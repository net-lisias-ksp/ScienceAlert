using System;
using ScienceAlert.VesselContext.Experiments.Sensors;

namespace ScienceAlert.VesselContext.Experiments
{
    [Implements(typeof(IExperimentSensorMonitorFactory))]
// ReSharper disable once UnusedMember.Global
    public class ExperimentSensorMonitorFactory : IExperimentSensorMonitorFactory
    {
        private readonly ISensorFactory _sensorFactory;
        private readonly SignalExperimentSensorStatusChanged _changedSignal;

        public ExperimentSensorMonitorFactory(ISensorFactory sensorFactory, SignalExperimentSensorStatusChanged changedSignal)
        {
            if (sensorFactory == null) throw new ArgumentNullException("sensorFactory");
            if (changedSignal == null) throw new ArgumentNullException("changedSignal");
            _sensorFactory = sensorFactory;
            _changedSignal = changedSignal;
        }

        public IExperimentSensorMonitor Create(ScienceExperiment experiment)
        {
            if (experiment == null) throw new ArgumentNullException("experiment");

            return new ExperimentSensorMonitor(
                experiment,
                _changedSignal,
                _sensorFactory.CreateOnboardSensor(experiment),
                _sensorFactory.CreateAvailabilitySensor(experiment),
                _sensorFactory.CreateCollectionSensor(experiment),
                _sensorFactory.CreateTransmissionSensor(experiment),
                _sensorFactory.CreateLabDataSensor(experiment));
        }
    }
}
