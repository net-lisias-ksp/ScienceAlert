using System;
using System.Text;
using ScienceAlert.Game;
using ScienceAlert.VesselContext.Experiments.Rules;
using UnityEngine;

namespace ScienceAlert.VesselContext.Experiments
{
    public class ExperimentSensor : IExperimentSensor, IExperimentSensorState//, IEquatable<IExperimentSensor>
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
            IExperimentRule conditionRule, 
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

        public IExperimentSensorState State
        {
            get { return this; }
        }


        public void UpdateSensorValues()
        {
            Profiler.BeginSample("ExperimentSensor.UpdateSensorValues");

            Profiler.BeginSample("ExperimentSensor.UpdateSensorValues.GetSubject");
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

            Profiler.EndSample();
        }


        private void UpdateCollectionValue()
        {
            Profiler.BeginSample("ExperimentSensor.UpdateCollectionValue");
            CollectionValue = _reportCalculator.CalculateCollectionValue(Experiment, Subject);
            Profiler.EndSample();
        }


        private void UpdateTransmissionValue()
        {
            Profiler.BeginSample("ExperimentSensor.UpdateTransmissionValue");
            TransmissionValue = _reportCalculator.CalculateTransmissionValue(Experiment, Subject);
            Profiler.EndSample();
        }


        private void UpdateLabValue()
        {
            Profiler.BeginSample("ExperimentSensor.UpdateLabValue");
            LabValue = _reportCalculator.CalculateLabValue(Experiment, Subject);
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


        public bool Equals(IExperimentSensorState report)
        {
            return ReferenceEquals(Experiment, report.Experiment) &&
                   Mathf.Approximately(CollectionValue, report.CollectionValue) &&
                   Mathf.Approximately(TransmissionValue, report.TransmissionValue) &&
                   Mathf.Approximately(LabValue, report.LabValue) &&
                   Onboard == report.Onboard &&
                   Available == report.Available &&
                   ConditionsMet == report.ConditionsMet;
        }


        public bool Equals(IExperimentSensor other)
        {
            if (other == null) return false;
            return Experiment.id == other.Experiment.id;
        }


        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 137;

                hash = hash * 479 + Experiment.GetHashCode();
                hash = hash * 479 + CollectionValue.GetHashCode();
                hash = hash * 479 + TransmissionValue.GetHashCode();
                hash = hash * 479 + Onboard.GetHashCode();
                hash = hash * 479 + Available.GetHashCode();
                hash = hash * 479 + ConditionsMet.GetHashCode();
                hash = hash * 479 + Subject.Id.GetHashCode();

                return hash;
            }
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
