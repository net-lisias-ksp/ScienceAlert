using System;
using System.Text;
using ScienceAlert.Game;
using ScienceAlert.VesselContext.Experiments.Rules;
using UnityEngine;

namespace ScienceAlert.VesselContext.Experiments
{
    public class ExperimentSensor : IExperimentSensor
    {
        private readonly IScienceSubjectProvider _scienceSubjectProvider;
        private readonly IExperimentReportValueCalculator _reportCalculator;
        private readonly ISensorRule _onboardRule;
        private readonly ISensorRule _availableRule;
        private readonly ISensorRule _conditionRule;

        public ExperimentSensor(
            ScienceExperiment experiment, 
            IScienceSubjectProvider scienceSubjectProvider,
            IExperimentReportValueCalculator reportCalculator,
            ISensorRule onboardRule,
            ISensorRule availableRule,
            ISensorRule conditionRule, 
            IScienceSubject initialSubject)
        {
            if (experiment == null) throw new ArgumentNullException("experiment");
            if (scienceSubjectProvider == null) throw new ArgumentNullException("scienceSubjectProvider");
            if (reportCalculator == null) throw new ArgumentNullException("reportCalculator");
            if (onboardRule == null) throw new ArgumentNullException("onboardRule");
            if (availableRule == null) throw new ArgumentNullException("availableRule");
            if (conditionRule == null) throw new ArgumentNullException("conditionRule");
            if (initialSubject == null) throw new ArgumentNullException("initialSubject");

            Experiment = experiment;
            _scienceSubjectProvider = scienceSubjectProvider;
            _reportCalculator = reportCalculator;
            _onboardRule = onboardRule;
            _availableRule = availableRule;
            _conditionRule = conditionRule;
            Subject = initialSubject;
        }


        public bool HasChanged { get; private set; }
        public float CollectionValue { get; private set; }
        public float TransmissionValue { get; private set; }
        public float LabValue { get; private set; }
        public bool Onboard { get; private set; }               // related module is actually onboard the vessel? (onboard rule check)
        public bool Available { get; private set; }             // is at least one module available for deployment? (availability rule check)
        public bool ConditionsMet { get; private set; }              // Can the related experiment actually be run (runnable rule check)
        public IScienceSubject Subject { get; private set; }
        public ScienceExperiment Experiment { get; private set; }

        public void ClearChangedFlag()
        {
            HasChanged = false;
        }

        public ExperimentSensorState State
        {
            get
            {
                return new ExperimentSensorState(Experiment, Subject, CollectionValue, TransmissionValue, LabValue,
                    Onboard, Available, ConditionsMet);
            }
        }


        public void UpdateSensorValues()
        {
            var oldSubject = Subject;
            Subject = _scienceSubjectProvider.GetSubject(Experiment);
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
                         oldSubject.Id != Subject.Id;
        }


        private void UpdateCollectionValue()
        {
            CollectionValue = _reportCalculator.CalculateCollectionValue(Experiment, Subject);
        }


        private void UpdateTransmissionValue()
        {
            TransmissionValue = _reportCalculator.CalculateTransmissionValue(Experiment, Subject);
        }


        private void UpdateLabValue()
        {
            LabValue = _reportCalculator.CalculateLabValue(Experiment, Subject);
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


        public override string ToString()
        {
            var builder = new StringBuilder(128);

            builder.AppendFormat(
                "ExperimentSensor ({0}): Onboard {1}, Available {2}, Condition {3}, Collection {4}, Transmission {5}, Lab {6}",
                Experiment.id, Onboard, Available, ConditionsMet, CollectionValue, TransmissionValue, LabValue);

            return builder.ToString();
        }
    }
}
