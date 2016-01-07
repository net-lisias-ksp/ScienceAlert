using System;
using System.Collections.Generic;
using System.Linq;
using ScienceAlert.VesselContext.Experiments.Sensors;

namespace ScienceAlert.VesselContext.Experiments
{
    /// <summary>
    /// This is the hub where all the various sensors connected to a particular sensor meet. If any of them
    /// change, a new sensor status update will be dispatched
    /// </summary>
    public class ExperimentSensorMonitor : IExperimentSensorMonitor, IExperimentSensorStateFactory
    {
        private readonly ScienceExperiment _experiment;
        private readonly SignalExperimentSensorStatusChanged _statusChangedSignal;
        private readonly IOnboardSensor _onboardSensor;
        private readonly IAvailabilitySensor _availabilitySensor;
        private readonly ICollectionSensor _collectionSensor;
        private readonly ITransmissionSensor _transmissionSensor;
        private readonly ILabDataSensor _labSensor;

        private bool _firstUpdate = true;
        private readonly List<ISensor> _sensorList = new List<ISensor>();
 
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

            // makes the update method just a bit cleaner
            _sensorList = new List<ISensor>
            {
                _onboardSensor,
                _availabilitySensor,
                _collectionSensor,
                _transmissionSensor,
                _labSensor
            };
        }


        public void UpdateSensorStates()
        {
            _sensorList.ForEach(s => s.ClearChangedFlag());
            _sensorList.ForEach(s => s.Update());

            if (_firstUpdate || _sensorList.Any(s => s.HasChanged))
                DispatchStateChangeSignal();

            _firstUpdate = false;
        }


        private void DispatchStateChangeSignal()
        {
            _statusChangedSignal.Dispatch(GetState());
        }


        public ExperimentSensorState GetState()
        {
            return new ExperimentSensorState(_experiment, _collectionSensor.Value, _transmissionSensor.Value,
                _labSensor.Value, _onboardSensor.Value, _availabilitySensor.Value);
        }
    }
}
