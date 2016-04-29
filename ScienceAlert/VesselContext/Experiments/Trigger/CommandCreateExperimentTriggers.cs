using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using ReeperCommon.Extensions;
using ReeperCommon.Logging;
using ReeperKSP.Extensions;
using ScienceAlert.SensorDefinitions;
using strange.extensions.command.impl;

namespace ScienceAlert.VesselContext.Experiments.Trigger
{
// ReSharper disable once ClassNeverInstantiated.Global
    public class CommandCreateExperimentTriggers : Command
    {
        private readonly IEnumerable<SensorDefinition> _definitions;
        private readonly ITriggerBuilder _triggerBuilder;
        private readonly ITemporaryBindingFactory _bindingFactory;
        private static bool _loggedInvalidTriggers = false;

        public CommandCreateExperimentTriggers(
            ReadOnlyCollection<SensorDefinition> definitions, 
            ITriggerBuilder triggerBuilder,
            ITemporaryBindingFactory bindingFactory)
        {
            if (definitions == null) throw new ArgumentNullException("definitions");
            if (triggerBuilder == null) throw new ArgumentNullException("triggerBuilder");
            if (bindingFactory == null) throw new ArgumentNullException("bindingFactory");

            _definitions = definitions;
            _triggerBuilder = triggerBuilder;
            _bindingFactory = bindingFactory;
        }


        public override void Execute()
        {
            Log.TraceMessage();

            LogInvalidTriggers();

            var triggers = new List<ExperimentTrigger>();

            foreach (var triggerDefinition in _definitions)
            {
                try
                {
                    triggers.Add(CreateTrigger(triggerDefinition));
                }
                catch (Exception e)
                {
                    Log.Error("Failed to create trigger for " + triggerDefinition.Experiment.id + " due to: " + e);
                }
            }

            injectionBinder.Bind<IEnumerable<ExperimentTrigger>>().Bind<List<ExperimentTrigger>>().To(triggers);
            Log.Verbose("Created " + triggers.Count + " ExperimentTriggers");
        }


        private void LogInvalidTriggers()
        {
            if (_loggedInvalidTriggers) return;

            _loggedInvalidTriggers = true;

            var invalids = GetDefinitionsWithInvalidTriggerDefinitions().ToList();

            if (invalids.Any())
            {
                Log.Debug("Logging invalid triggers");

                invalids
                    .ForEach(sd =>
                            Log.Error("Can't create trigger for " + sd.Experiment.id + ": " +
                                      sd.TriggerDefinition.ToSafeString()));
            }
        }


        private IEnumerable<SensorDefinition> GetDefinitionsWithInvalidTriggerDefinitions()
        {
            return _definitions.Where(sd => !_triggerBuilder.CanHandle(sd.TriggerDefinition));
        }


        private ExperimentTrigger CreateTrigger(SensorDefinition definition)
        {
            try
            {
                injectionBinder.Bind<ScienceExperiment>().To(definition.Experiment);

                return _triggerBuilder.Build(definition.TriggerDefinition, _triggerBuilder, injectionBinder,
                    _bindingFactory);
            }
            finally
            {
                injectionBinder.Unbind<ScienceExperiment>();
            }
        }
    }
}
