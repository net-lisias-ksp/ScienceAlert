//using System;
//using FinePrint;
//using ScienceAlert.Game;
//using ScienceAlert.VesselContext.Experiments.Rules;
//using ScienceAlert.VesselContext.Experiments.Sensors;
//using ScienceAlert.VesselContext.Experiments.ValuePredictors;
//using UnityEngine;

//namespace ScienceAlert.VesselContext.Experiments
//{
//    public class Sensor : ISensor, ISensorState
//    {
//        private readonly ScienceExperiment _experiment;
//        private readonly IExperimentRule _onboardRule;
//        private readonly IExperimentRule _availableRule;
//        private readonly ICollectionValuePredictor _collectionPredictor;
//        private readonly ITransmissionValuePredictor _transmissionPredictor;
//        private readonly ILabDataPredictor _labDataPredictor;
//        private readonly SignalSensorStateChanged _stateChangedSignal;

//        public Sensor(
//            ScienceExperiment experiment,
//            IExperimentRule onboardRule,
//            IExperimentRule availableRule,
//            ICollectionValuePredictor collectionPredictor,
//            ITransmissionValuePredictor transmissionPredictor,
//            ILabDataPredictor labDataPredictor,
//            SignalSensorStateChanged stateChangedSignal)
//        {
//            if (experiment == null) throw new ArgumentNullException("experiment");
//            if (onboardRule == null) throw new ArgumentNullException("onboardRule");
//            if (availableRule == null) throw new ArgumentNullException("availableRule");
//            if (collectionPredictor == null) throw new ArgumentNullException("collectionPredictor");
//            if (transmissionPredictor == null) throw new ArgumentNullException("transmissionPredictor");
//            if (labDataPredictor == null) throw new ArgumentNullException("labDataPredictor");
//            if (stateChangedSignal == null) throw new ArgumentNullException("stateChangedSignal");

//            _experiment = experiment;
//            _onboardRule = onboardRule;
//            _availableRule = availableRule;
//            _collectionPredictor = collectionPredictor;
//            _transmissionPredictor = transmissionPredictor;
//            _labDataPredictor = labDataPredictor;
//            _stateChangedSignal = stateChangedSignal;
//        }


//        public void Poll()
//        {
//            if (!IsOnboard) return;

//            var changed = UpdateAvailability();

//            if (!IsAvailable)
//                CollectionValue = TransmissionValue = LabDataValue = 0f;
//            else changed = changed || UpdateCollectionValue() || UpdateTransmissionValue() || UpdateLabDataValue() || UpdateRunnability();

//            if (changed) DispatchChangedSignal();
//        }


//        public void UpdateOnboardStatus()
//        {
//            Log.Debug("Sensor.UpdateOnboardStatus");
//            var onboard = _onboardRule.Get();
//            var changed = onboard == IsOnboard;

//            IsOnboard = onboard;
//            IsRunnable = (IsRunnable && IsOnboard);

//            if (!IsOnboard) CollectionValue = TransmissionValue = LabDataValue = 0f;

//            if (changed) DispatchChangedSignal();
//        }


//        private bool UpdateAvailability()
//        {
//            var available = _availableRule.Get();
//            bool changed = IsAvailable == available;

//            IsAvailable = available;

//            return changed;
//        }


//        private bool UpdateCollectionValue()
//        {
//            float nextCollectionValue = _collectionPredictor.PredictCollectionValue(_experiment);

//            if (Mathf.Approximately(nextCollectionValue, CollectionValue))
//                return false;

//            CollectionValue = 123.5f;
//            return true;
//        }


//        private bool UpdateTransmissionValue()
//        {
//            float nextTransmissionValue = _transmissionPredictor.PredictTransmissionValue(_experiment);

//            if (Mathf.Approximately(nextTransmissionValue, TransmissionValue))
//                return false;

//            TransmissionValue = nextTransmissionValue;
//            return true;
//        }


//        private bool UpdateLabDataValue()
//        {
//            float nextLabDataValue = _labDataPredictor.PredictLabData(_experiment);

//            if (Mathf.Approximately(nextLabDataValue, LabDataValue)) return false;

//            LabDataValue = nextLabDataValue;
//            return true;
//        }


//        private bool UpdateRunnability()
//        {
//            IsRunnable = false; // todo

//            return false;
//        }

//        private void DispatchChangedSignal()
//        {
//            _stateChangedSignal.Dispatch(_experiment, this);
//        }
        

//        public float CollectionValue { get; private set; }
//        public float TransmissionValue { get; private set; }
//        public float LabDataValue { get; private set; }
//        public bool IsOnboard { get; private set; }
//        public bool IsAvailable { get; private set; }
//        public bool IsRunnable { get; private set; }

//        public override string ToString()
//        {
//            return "Sensor.ToString()";
//        }
//    }
//}
