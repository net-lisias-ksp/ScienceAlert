using System;
using ScienceAlert.Game;
using ScienceAlert.VesselContext.Experiments.Rules;
using UnityEngine;

namespace ScienceAlert.VesselContext.Experiments
{
    public class ExperimentSensor
    {
        private readonly IScienceSubjectProvider _scienceSubjectProvider;
        private readonly IExperimentReportValueCalculator _reportCalculator;
        private readonly IExperimentRule _onboardRule;
        private readonly IExperimentRule _availableRule;
        private readonly IExperimentRule _conditionRule;


        public ExperimentSensor(
            ScienceExperiment experiment, 
            IScienceSubjectProvider scienceSubjectProvider,
            IExperimentReportValueCalculator reportCalculator,
            IExperimentRule onboardRule,
            IExperimentRule availableRule,
            IExperimentRule conditionRule)
        {
            if (experiment == null) throw new ArgumentNullException("experiment");
            if (scienceSubjectProvider == null) throw new ArgumentNullException("scienceSubjectProvider");
            if (reportCalculator == null) throw new ArgumentNullException("reportCalculator");
            if (onboardRule == null) throw new ArgumentNullException("onboardRule");
            if (availableRule == null) throw new ArgumentNullException("availableRule");
            if (conditionRule == null) throw new ArgumentNullException("conditionRule");

            Experiment = experiment;
            _scienceSubjectProvider = scienceSubjectProvider;
            _reportCalculator = reportCalculator;
            _onboardRule = onboardRule;
            _availableRule = availableRule;
            _conditionRule = conditionRule;
        }


        public bool HasChanged { get; private set; }
        public float CollectionValue { get; private set; }
        public float TransmissionValue { get; private set; }
        public float LabValue { get; private set; }
        public bool Onboard { get; private set; }               // related module is actually onboard the vessel? (onboard rule check)
        public bool Available { get; private set; }             // is at least one module available for deployment? (availability rule check)
        public bool ConditionsMet { get; private set; }              // Can the related experiment actually be run (runnable rule check)
        public IScienceSubject CurrentSubject { get; private set; }
        public ScienceExperiment Experiment { get; private set; }

        public void ClearChangedFlag()
        {
            HasChanged = false;
        }


        public void UpdateSensorValues()
        {
            Profiler.BeginSample("ExperimentSensor.UpdateSensorValues");

            Profiler.BeginSample("ExperimentSensor.UpdateSensorValues.GetSubject");
            var oldSubject = CurrentSubject;
            CurrentSubject = _scienceSubjectProvider.GetSubject(Experiment);
            Profiler.EndSample();

            var oldConditionsMet = ConditionsMet;
            var oldOnboard = Onboard;
            var oldAvailable = Available;
            var oldCollection = CollectionValue;
            var oldTransmission = TransmissionValue;
            var oldLab = LabValue;
            
            UpdateConditionValue();
            UpdateOnboardValue();
            UpdateAvailabilityValue();

            // The experiment isn't valid for these conditions so there's no sensible science value to calculate
            if (!ConditionsMet)
            {
                CollectionValue = TransmissionValue = LabValue = 0f;
                Available = false;
            }
            else
            {
                UpdateCollectionValue();
                UpdateTransmissionValue();
                UpdateLabValue();
            }

            HasChanged = !Mathf.Approximately(oldCollection, CollectionValue) ||
                         !Mathf.Approximately(oldTransmission, TransmissionValue) ||
                         !Mathf.Approximately(oldLab, LabValue) || oldOnboard != Onboard || oldAvailable != Available || oldConditionsMet != ConditionsMet ||
                         oldSubject.Id != CurrentSubject.Id;

            Profiler.EndSample();
        }


        private void UpdateCollectionValue()
        {
            Profiler.BeginSample("ExperimentSensor.UpdateCollectionValue");
            CollectionValue = _reportCalculator.CalculateCollectionValue(Experiment, CurrentSubject);
            Profiler.EndSample();
        }


        private void UpdateTransmissionValue()
        {
            Profiler.BeginSample("ExperimentSensor.UpdateTransmissionValue");
            TransmissionValue = _reportCalculator.CalculateTransmissionValue(Experiment, CurrentSubject);
            Profiler.EndSample();
        }


        private void UpdateLabValue()
        {
            Profiler.BeginSample("ExperimentSensor.UpdateLabValue");
            LabValue = _reportCalculator.CalculateLabValue(Experiment, CurrentSubject);
            Profiler.EndSample();
        }


        private void UpdateOnboardValue()
        {
            Profiler.BeginSample("ExperimentSensor.UpdateOnboardValue");
            Onboard = _onboardRule.Passes();
            Profiler.EndSample();
        }


        private void UpdateAvailabilityValue()
        {
            Profiler.BeginSample("ExperimentSensor.UpdateAvailabilityValue");
            Available = _availableRule.Passes();
            Profiler.EndSample();
        }


        private void UpdateConditionValue()
        {
            Profiler.BeginSample("ExperimentSensor.UpdateConditionValue");
            ConditionsMet = _conditionRule.Passes();
            Profiler.EndSample();
        }
    }
}
