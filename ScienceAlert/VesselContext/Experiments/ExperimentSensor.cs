using System;
using System.Linq;
using ScienceAlert.Game;
using ScienceAlert.VesselContext.Experiments.Rules;
using UnityEngine;

namespace ScienceAlert.VesselContext.Experiments
{
    public class ExperimentSensor
    {
        public readonly ScienceExperiment Experiment;
        private readonly IScienceSubjectProvider _scienceSubjectProvider;
        private readonly IExperimentReportValueCalculator _reportCalculator;
        private readonly IExperimentRule _onboardRule;
        private readonly IExperimentRule _availableRule;
        private readonly IExperimentRule _conditionRule;

        private IScienceSubject _currentSubject;
        

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


        public void ClearChangedFlag()
        {
            HasChanged = false;
        }


        public void UpdateSensorValues()
        {
            _currentSubject = _scienceSubjectProvider.GetSubject(Experiment);

            var oldConditionsMet = ConditionsMet;
            var oldOnboard = Onboard;
            var oldAvailable = Available;
            var oldCollection = CollectionValue;
            var oldTransmission = TransmissionValue;
            var oldLab = LabValue;
            
            UpdateConditionValue();
            UpdateOnboardValue();
            UpdateAvailabilityValue();

            if (!ConditionsMet)
            {
                CollectionValue = TransmissionValue = LabValue = 0f;
            }
            else
            {
                UpdateCollectionValue();
                UpdateTransmissionValue();
                UpdateLabValue();
            }

            HasChanged = !Mathf.Approximately(oldCollection, CollectionValue) ||
                         !Mathf.Approximately(oldTransmission, TransmissionValue) ||
                         !Mathf.Approximately(oldLab, LabValue) || oldOnboard != Onboard || oldAvailable != Available || oldConditionsMet != ConditionsMet;
        }


        private void UpdateCollectionValue()
        {
            CollectionValue = _reportCalculator.CalculateCollectionValue(Experiment, _currentSubject);
        }


        private void UpdateTransmissionValue()
        {
            TransmissionValue = _reportCalculator.CalculateTransmissionValue(Experiment, _currentSubject);
        }


        private void UpdateLabValue()
        {
            LabValue = _reportCalculator.CalculateLabValue(Experiment, _currentSubject);
        }


        private void UpdateOnboardValue()
        {
            Onboard = _onboardRule.Passes();
        }


        private void UpdateAvailabilityValue()
        {
            Available = _availableRule.Passes();
        }


        private void UpdateConditionValue()
        {
            ConditionsMet = _conditionRule.Passes();
        }
    }
}
