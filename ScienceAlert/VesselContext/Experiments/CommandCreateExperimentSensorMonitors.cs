using System;
using System.Collections.Generic;
using System.Linq;
using ScienceAlert.VesselContext.Experiments.Sensors;
using strange.extensions.command.impl;

namespace ScienceAlert.VesselContext.Experiments
{
// ReSharper disable once ClassNeverInstantiated.Global
    public class CommandCreateExperimentSensorMonitors : Command
    {
        private readonly IEnumerable<ScienceExperiment> _experiments;
        private readonly SignalExperimentSensorStatusChanged _statusChangedSignal;
        private readonly ISensorFactory _sensorFactory;


        public CommandCreateExperimentSensorMonitors(
            IEnumerable<ScienceExperiment> experiments,
            SignalExperimentSensorStatusChanged statusChangedSignal,
            ISensorFactory sensorFactory)
        {
            if (experiments == null) throw new ArgumentNullException("experiments");
            if (statusChangedSignal == null) throw new ArgumentNullException("statusChangedSignal");
            if (sensorFactory == null) throw new ArgumentNullException("sensorFactory");
            _experiments = experiments;
            _statusChangedSignal = statusChangedSignal;
            _sensorFactory = sensorFactory;
        }


        public override void Execute()
        {
            var monitorsForEachExperiment = _experiments.ToDictionary(se => se, Create);

            foreach (var kvp in monitorsForEachExperiment)
                injectionBinder
                    .Bind<IExperimentSensorStateFactory>()
                    .Bind<IExperimentSensorMonitor>()
                    .ToValue(kvp.Value)
                    .ToName(kvp.Key);

            var monitorList = monitorsForEachExperiment.Values.ToList();

            var factoryList = monitorList.Cast<IExperimentSensorStateFactory>().ToList();
            var imonitorList = monitorList.Cast<IExperimentSensorMonitor>().ToList();

            injectionBinder.Bind<IEnumerable<IExperimentSensorStateFactory>>()
                .Bind<List<IExperimentSensorStateFactory>>()
                .ToValue(factoryList);

            injectionBinder.Bind<IEnumerable<IExperimentSensorMonitor>>()
                .Bind<List<IExperimentSensorMonitor>>()
                .ToValue(imonitorList);

            //injectionBinder
            //    .Bind<IEnumerable<IExperimentSensorStateFactory>>()
            //    .Bind<List<IExperimentSensorMonitor>>()
            //    .ToValue(monitorList);
        }


        private ExperimentSensorMonitor Create(ScienceExperiment experiment)
        {
            if (experiment == null) throw new ArgumentNullException("experiment");

            Log.Debug("Creating ExperimentSensorMonitor for " + experiment.id);

            return new ExperimentSensorMonitor(experiment, _statusChangedSignal,
                _sensorFactory.CreateOnboardSensor(experiment),
                _sensorFactory.CreateAvailabilitySensor(experiment),
                _sensorFactory.CreateCollectionSensor(experiment),
                _sensorFactory.CreateTransmissionSensor(experiment),
                _sensorFactory.CreateLabDataSensor(experiment));
        }
    }
}
