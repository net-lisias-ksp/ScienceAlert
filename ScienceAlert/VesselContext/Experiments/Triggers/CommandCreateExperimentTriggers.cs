using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using JetBrains.Annotations;
using ReeperCommon.Containers;
using ReeperCommon.Extensions;
using ReeperCommon.Logging;
using strange.extensions.command.impl;
using strange.extensions.injector.api;
using UnityEngine;

namespace ScienceAlert.VesselContext.Experiments.Triggers
{
    // ReSharper disable once ClassNeverInstantiated.Global
    class CommandCreateExperimentTriggers : Command
    {
        private readonly ReadOnlyCollection<ExperimentConfiguration> _configurations;
        private readonly IObjectFromConfigNodeBuilder<IExperimentTrigger, ExperimentConfiguration, IInjectionBinder> _triggerBuilder;
        private readonly ITemporaryBindingFactory _tempBinder;
        private readonly ICriticalShutdownEvent _shutdown;

        private readonly Lazy<ReadOnlyCollection<Type>> _allTriggerTypes = new Lazy<ReadOnlyCollection<Type>>(GetAllTriggerTypes);



        public CommandCreateExperimentTriggers(
            [NotNull] ReadOnlyCollection<ExperimentConfiguration> configurations,
            [NotNull] IObjectFromConfigNodeBuilder<IExperimentTrigger, ExperimentConfiguration, IInjectionBinder> triggerBuilder,
            [NotNull] ITemporaryBindingFactory tempBinder,
            [NotNull] ICriticalShutdownEvent shutdown)
        {
            if (configurations == null) throw new ArgumentNullException("configurations");
            if (triggerBuilder == null) throw new ArgumentNullException("triggerBuilder");
            if (tempBinder == null) throw new ArgumentNullException("tempBinder");
            if (shutdown == null) throw new ArgumentNullException("shutdown");
            _configurations = configurations;
            _triggerBuilder = triggerBuilder;
            _tempBinder = tempBinder;
            _shutdown = shutdown;
        }


        public override void Execute()
        {
            CreateTriggersForEveryExperimentConfiguration();
        }


        private void CreateTriggersForEveryExperimentConfiguration()
        {
            int triggerCount = 0;

            foreach (var config in _configurations)
            {
                try
                {
                    if (injectionBinder.GetBinding<IExperimentTrigger>(config.Experiment.id) != null ||
                        injectionBinder.GetBinding<IExperimentTrigger>(config.Experiment) != null)
                    {
                        Log.Error("Already have a trigger binding for " + config.Experiment.id + "!");
                        continue;
                    }

                    var trigger = CreateTrigger(config);

                    injectionBinder.Bind<IExperimentTrigger>().To(trigger).ToName(config.Experiment);
                    injectionBinder.Bind<IExperimentTrigger>().To(trigger).ToName(config.Experiment.id);

                    Log.Verbose("Created trigger for " + config.Experiment.id);
                    ++triggerCount;
                }
                catch (Exception e)
                {
                    Log.Error("Failed to create experiment trigger for " + config + " due to " + e);
                }
            }

            if (triggerCount == 0)
            {
                Log.Error("No triggers were created! Something has broken.");
                Fail();
                _shutdown.Dispatch();
                return;
            }

            Log.Debug("Created " + triggerCount + " triggers");
        }


        private IExperimentTrigger CreateTrigger(ExperimentConfiguration configuration)
        {
            var triggerType = GetTriggerType(configuration);

            if (!triggerType.Any()) // failed to find the trigger user specified
            {
                Log.Warning("Unable to find a trigger called '" + configuration.TriggerDefinition.Return(d => d, "<none specified>") +
                            "' for " + configuration.Experiment.id + ". A default trigger will be used");
                triggerType = Maybe<Type>.With(typeof (DefaultExperimentTrigger));
            }


            try
            {
                return CreateTriggerByType(configuration, triggerType.Value);
            }
            catch (Exception e)
            {
                Log.Error("Unable to create trigger '" + triggerType.Value.Name + "' due to " + e);

                // can we try again with the default? if not, something is terribly hosed
                if (triggerType.Value == typeof (DefaultExperimentTrigger)) throw;

                Log.Warning("Attempting to create a default trigger instead");
                return CreateTriggerByType(configuration, typeof (DefaultExperimentTrigger));
            }
        }


        private IExperimentTrigger CreateTriggerByType(ExperimentConfiguration configuration, Type triggerType)
        {
            if (!typeof (IExperimentTrigger).IsAssignableFrom(triggerType))
                throw new ArgumentException("must specify a type that implements " + typeof (IExperimentTrigger).Name,
                    "triggerType");

            using (_tempBinder.Create(injectionBinder, typeof (ScienceExperiment), configuration.Experiment))
            using (var binding = _tempBinder.Create(injectionBinder, triggerType))
                return (IExperimentTrigger) binding.GetInstance();
        }


        private Maybe<Type> GetTriggerType(ExperimentConfiguration configuration)
        {
            // if none was specified, assume the default is desired
            if (!configuration.TriggerDefinition.Any()) return Maybe<Type>.With(typeof (DefaultExperimentTrigger));

            // find the type of the specified trigger, if it exists
            return configuration
                .With(c => c.TriggerDefinition.Value)
                .With(
                    triggerName =>
                        _allTriggerTypes.Value.FirstOrDefault(
                            triggerTy => triggerTy.Name.Equals(triggerName, StringComparison.OrdinalIgnoreCase)))
                            .ToMaybe();
        }



        private static ReadOnlyCollection<Type> GetAllTriggerTypes()
        {
            var startTime = Time.realtimeSinceStartup;

            try
            {
                return AssemblyLoader.loadedAssemblies.SelectMany(la => la.assembly.GetTypes())
                    .Where(t => !t.IsAbstract && t.HasInterface<IExperimentTrigger>())
                    .ToList()
                    .AsReadOnly();
            }
            finally
            {
                Log.Warning("Time spent scanning trigger types: " +
                            (Time.realtimeSinceStartup - startTime).ToString("F3"));
            }
        }
    }
}
