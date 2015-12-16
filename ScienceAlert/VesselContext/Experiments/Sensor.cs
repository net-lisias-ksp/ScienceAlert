using System;
using ScienceAlert.Rules;

namespace ScienceAlert.VesselContext.Experiments
{
    public class Sensor : ISensor//, ISensorValues
    {
        private readonly IExperimentRule _onboardRule;
        private readonly IExperimentRule _availableRule;

        public Sensor(
            ScienceExperiment experiment,
            IExperimentRule onboardRule,
            IExperimentRule availableRule)
        {
            if (experiment == null) throw new ArgumentNullException("experiment");
            if (onboardRule == null) throw new ArgumentNullException("onboardRule");
            if (availableRule == null) throw new ArgumentNullException("availableRule");

            Experiment = experiment;
            _onboardRule = onboardRule;
            _availableRule = availableRule;
        }


        public void Poll()
        {
            if (!IsOnboard) return;

            var changed = UpdateAvailability();

            if (!IsAvailable)
                CollectionValue = TransmissionValue = LabDataValue = 0f;
            else changed = changed || UpdateCollectionValue() || UpdateTransmissionValue() || UpdateLabDataValue();

            if (changed) DispatchChangedSignal();
        }


        public void OnVesselModified()
        {
            Log.Warning("Sensor.OnVesselModified");
            UpdateOnboardAvailability();
        }

        
        
        public void UpdateOnboardAvailability()
        {
            Log.Debug("Sensor.OnVesselModified");
            var onboard = _onboardRule.Get();
            var changed = onboard == IsOnboard;

            IsOnboard = onboard;

            if (!IsOnboard) CollectionValue = TransmissionValue = LabDataValue = 0f;

            if (changed) DispatchChangedSignal();
        }


        private bool UpdateAvailability()
        {
            var available = _availableRule.Get();
            bool changed = IsAvailable == available;

            IsAvailable = available;

            return changed;
        }



        private bool UpdateCollectionValue()
        {
            // todo
            CollectionValue = 123.5f;
            return false;
        }

        private bool UpdateTransmissionValue()
        {
            TransmissionValue = 0f;
            return false;
        }

        private bool UpdateLabDataValue()
        {
            LabDataValue = 0f;
            return false;
        }


        private void DispatchChangedSignal()
        {
            //_sensorChangedSignal.Dispatch(Experiment, this);
        }
        

        public float CollectionValue { get; private set; }
        public float TransmissionValue { get; private set; }
        public float LabDataValue { get; private set; }
        public bool IsOnboard { get; private set; }
        public bool IsAvailable { get; private set; }
        public bool IsRunnable { get; private set; }
        public ScienceExperiment Experiment { get; private set; }
    }
}
