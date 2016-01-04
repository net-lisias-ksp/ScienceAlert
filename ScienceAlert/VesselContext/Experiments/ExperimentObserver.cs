using System;
using ScienceAlert.VesselContext.Experiments.Sensors;

namespace ScienceAlert.VesselContext.Experiments
{
    public class ExperimentObserver : IExperimentObserver
    {
        private readonly ScienceExperiment _experiment;
        private readonly SignalExperimentStatusChanged _statusChangedSignal;
        private readonly IOnboardSensor _onboardSensor;
        private readonly IAvailabilitySensor _availabilitySensor;
        private readonly ICollectionSensor _collectionSensor;
        private readonly ITransmissionSensor _transmissionSensor;
        private readonly ILabDataSensor _labSensor;

        public ExperimentObserver(
            ScienceExperiment experiment,
            SignalExperimentStatusChanged statusChangedSignal,
            IOnboardSensor onboardSensor,
            IAvailabilitySensor availabilitySensor,
            ICollectionSensor collectionSensor,
            ITransmissionSensor transmissionSensor,
            ILabDataSensor labSensor)
        {
            if (experiment == null) throw new ArgumentNullException("experiment");
            if (statusChangedSignal == null) throw new ArgumentNullException("statusChangedSignal");
            if (onboardSensor == null) throw new ArgumentNullException("onboardSensor");
            if (availabilitySensor == null) throw new ArgumentNullException("availabilitySensor");
            if (collectionSensor == null) throw new ArgumentNullException("collectionSensor");
            if (transmissionSensor == null) throw new ArgumentNullException("transmissionSensor");
            if (labSensor == null) throw new ArgumentNullException("labSensor");

            _experiment = experiment;
            _statusChangedSignal = statusChangedSignal;
            _onboardSensor = onboardSensor;
            _availabilitySensor = availabilitySensor;
            _collectionSensor = collectionSensor;
            _transmissionSensor = transmissionSensor;
            _labSensor = labSensor;
        }


        public void Update()
        {
            bool onboardChanged = _onboardSensor.Poll();
            bool availableChanged = _availabilitySensor.Poll();

            bool collectionChanged = _collectionSensor.Poll();
            bool transmissionChanged = _transmissionSensor.Poll();
            bool labSensorChanged = _labSensor.Poll();

            // if any of the states have changed, we dispatch a signal to let anyone who cares know
            if (onboardChanged || availableChanged || collectionChanged || transmissionChanged || labSensorChanged)
                _statusChangedSignal.Dispatch(
                    new ExperimentStatusReport(_experiment, _collectionSensor.Value, _transmissionSensor.Value,
                        _labSensor.Value, _onboardSensor.Value, _availabilitySensor.Value));
        }
    }
}
