using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using JetBrains.Annotations;
using ReeperCommon.Containers;
using ReeperCommon.Logging;
using strange.extensions.command.impl;
using strange.extensions.injector.api;

namespace ScienceAlert.VesselContext.Experiments.Sensors
{
    // ReSharper disable once ClassNeverInstantiated.Global
    class CommandCreateExperimentSensors : Command
    {
        private readonly IObjectFromConfigNodeBuilder<IExperimentSensor, ExperimentConfiguration, IInjectionBinder> _sensorBuilder;
        private readonly ReadOnlyCollection<ExperimentConfiguration> _configurations;
        private readonly ICriticalShutdownEvent _shutdown;

        public CommandCreateExperimentSensors(
            [NotNull] IObjectFromConfigNodeBuilder<IExperimentSensor, ExperimentConfiguration, IInjectionBinder> sensorBuilder,
            [NotNull] ReadOnlyCollection<ExperimentConfiguration> configurations,
            [NotNull] ICriticalShutdownEvent shutdown)
        {
            if (sensorBuilder == null) throw new ArgumentNullException("sensorBuilder");
            if (configurations == null) throw new ArgumentNullException("configurations");
            if (shutdown == null) throw new ArgumentNullException("shutdown");
            _sensorBuilder = sensorBuilder;
            _configurations = configurations;
            _shutdown = shutdown;
        }

        public override void Execute()
        {
            var sensors = new List<IExperimentSensor>();

            foreach (var config in _configurations)
            {
                // TEMP!!!
                if (config.Experiment.id != "evaReport" && config.Experiment.id != "crewReport") continue;
                
                Log.Warning("Only creating sensor for EVA report and crew report");
                // >>>>ENDTEMP


                var sensor = CreateSensor(config);
                if (sensor.Any())
                    sensors.Add(sensor.Value);
            }

            if (!sensors.Any()) // something is definitely broken
            {
                Log.Error("No sensors were created!");
                _shutdown.Dispatch();
                Fail();
                return;
            }

            injectionBinder.Bind<ReadOnlyCollection<IExperimentSensor>>().To(sensors.AsReadOnly());
        }


        private Maybe<IExperimentSensor> CreateSensor(ExperimentConfiguration configuration)
        {
            try
            {
                return Maybe<IExperimentSensor>.With(_sensorBuilder.Build(configuration, injectionBinder));
            }
            catch (Exception e)
            {
                Log.Error("Exception while creating sensor for " + configuration.Experiment.id + ": " + e);
            }

            return Maybe<IExperimentSensor>.None;
        }
    }
}
