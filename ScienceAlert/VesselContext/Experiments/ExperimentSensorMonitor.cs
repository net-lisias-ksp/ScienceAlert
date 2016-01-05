using System;
using ScienceAlert.VesselContext.Experiments.Sensors;

namespace ScienceAlert.VesselContext.Experiments
{
    /// <summary>
    /// This is the hub where all the various sensors connected to a particular sensor meet. If any of them
    /// change, a new sensor status update will be dispatched
    /// </summary>
    public class ExperimentSensorMonitor : IExperimentSensorMonitor
    {
        private readonly ScienceExperiment _experiment;
        private readonly SignalExperimentSensorStatusChanged _statusChangedSignal;
        private readonly IOnboardSensor _onboardSensor;
        private readonly IAvailabilitySensor _availabilitySensor;
        private readonly ICollectionSensor _collectionSensor;
        private readonly ITransmissionSensor _transmissionSensor;
        private readonly ILabDataSensor _labSensor;

        private bool _firstUpdate = true;

        public ExperimentSensorMonitor(
            ScienceExperiment experiment,
            SignalExperimentSensorStatusChanged statusChangedSignal,
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
            _onboardSensor.Update();
            _availabilitySensor.Update();
            _collectionSensor.Update();
            _transmissionSensor.Update();
            _labSensor.Update();

            // if any of the states have changed, we dispatch a signal to let anyone who cares know
            if (_firstUpdate || _onboardSensor.HasChanged || _availabilitySensor.HasChanged || _collectionSensor.HasChanged ||
                _transmissionSensor.HasChanged || _labSensor.HasChanged)
                DispatchStateChangeSignal();

            _firstUpdate = false;
        }


        private void DispatchStateChangeSignal()
        {
            _statusChangedSignal.Dispatch(
                    new ExperimentSensorState(_experiment, _collectionSensor.Value, _transmissionSensor.Value,
                        _labSensor.Value, _onboardSensor.Value, _availabilitySensor.Value));
        }
    }
}
