using System;
using ScienceAlert.VesselContext.Experiments.Sensors;

namespace ScienceAlert.VesselContext.Experiments
{
    [Implements(typeof(IExperimentObserverFactory))]
// ReSharper disable once UnusedMember.Global
    public class ExperimentObserverFactory : IExperimentObserverFactory
    {
        private readonly ISensorFactory _sensorFactory;
        private readonly SignalExperimentStatusChanged _changedSignal;

        public ExperimentObserverFactory(ISensorFactory sensorFactory, SignalExperimentStatusChanged changedSignal)
        {
            if (sensorFactory == null) throw new ArgumentNullException("sensorFactory");
            if (changedSignal == null) throw new ArgumentNullException("changedSignal");
            _sensorFactory = sensorFactory;
            _changedSignal = changedSignal;
        }

        public IExperimentObserver Create(ScienceExperiment experiment)
        {
            if (experiment == null) throw new ArgumentNullException("experiment");

            return new ExperimentObserver(
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
