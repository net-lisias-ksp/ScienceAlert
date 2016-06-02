using System;
using System.Text;
using JetBrains.Annotations;
using ScienceAlert.Game;
using ScienceAlert.VesselContext.Experiments.Rules;
using UnityEngine;

namespace ScienceAlert.VesselContext.Experiments
{
    public class ExperimentSensor : IExperimentSensor
    {
        private readonly IExistingScienceSubjectProvider _scienceSubjectProvider;
        private readonly IExperimentReportValueCalculator _reportCalculator;
        private readonly IExperimentBiomeProvider _biomeProvider;
        private readonly IExperimentSituationProvider _situationProvider;
        private readonly ICelestialBodyProvider _bodyProvider;
        private readonly ISensorRule _onboardRule;
        private readonly ISensorRule _availableRule;
        private readonly ISensorRule _conditionRule;

        public ExperimentSensor(
            ScienceExperiment experiment, 
            IExistingScienceSubjectProvider scienceSubjectProvider,
            IExperimentReportValueCalculator reportCalculator,
            [NotNull] IExperimentBiomeProvider biomeProvider,
            [NotNull] IExperimentSituationProvider situationProvider,
            [NotNull] ICelestialBodyProvider bodyProvider,
            ISensorRule onboardRule,
            ISensorRule availableRule,
            ISensorRule conditionRule, 
            IScienceSubject initialSubject)
        {
            if (experiment == null) throw new ArgumentNullException("experiment");
            if (scienceSubjectProvider == null) throw new ArgumentNullException("scienceSubjectProvider");
            if (reportCalculator == null) throw new ArgumentNullException("reportCalculator");
            if (biomeProvider == null) throw new ArgumentNullException("biomeProvider");
            if (situationProvider == null) throw new ArgumentNullException("situationProvider");
            if (bodyProvider == null) throw new ArgumentNullException("bodyProvider");
            if (onboardRule == null) throw new ArgumentNullException("onboardRule");
            if (availableRule == null) throw new ArgumentNullException("availableRule");
            if (conditionRule == null) throw new ArgumentNullException("conditionRule");
            if (initialSubject == null) throw new ArgumentNullException("initialSubject");

            Experiment = experiment;
            _scienceSubjectProvider = scienceSubjectProvider;
            _reportCalculator = reportCalculator;
            _biomeProvider = biomeProvider;
            _situationProvider = situationProvider;
            _bodyProvider = bodyProvider;
            _onboardRule = onboardRule;
            _availableRule = availableRule;
            _conditionRule = conditionRule;
            Subject = initialSubject;
        }


        public bool HasChanged { get; private set; }
        public float RecoveryValue { get; private set; }
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
                return new ExperimentSensorState(Experiment, Subject, RecoveryValue, TransmissionValue, LabValue,
                    Onboard, Available, ConditionsMet);
            }
        }


        public void UpdateSensorValues()
        {
            var oldSubject = Subject;
            Subject = _scienceSubjectProvider.GetExistingSubject(Experiment, _situationProvider.ExperimentSituation, _bodyProvider.OrbitingBody, _biomeProvider.Biome);

            var oldConditionsMet = ConditionsMet;
            var oldOnboard = Onboard;
            var oldAvailable = Available;
            var oldRecovery = RecoveryValue;
            var oldTransmission = TransmissionValue;
            var oldLab = LabValue;
            
            UpdateConditionValue();
            UpdateOnboardValue();
            UpdateAvailabilityValue();

            // The experiment isn't valid for these conditions so there's no sensible science value to calculate
            if (!ConditionsMet)
            {
                RecoveryValue = TransmissionValue = LabValue = 0f;
                Available = false;
            }
            else
            {
                UpdateRecoveryValue();
                UpdateTransmissionValue();
                UpdateLabValue();
            }

            HasChanged = !Mathf.Approximately(oldRecovery, RecoveryValue) ||
                         !Mathf.Approximately(oldTransmission, TransmissionValue) ||
                         !Mathf.Approximately(oldLab, LabValue) || oldOnboard != Onboard || oldAvailable != Available || oldConditionsMet != ConditionsMet ||
                         oldSubject.Id != Subject.Id;
        }


        private void UpdateRecoveryValue()
        {
            RecoveryValue = _reportCalculator.CalculateRecoveryValue(Experiment, Subject);
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
                "ExperimentSensor ({0}): Onboard {1}, Available {2}, Condition {3}, Recovery {4}, Transmission {5}, Lab {6}",
                Experiment.id, Onboard, Available, ConditionsMet, RecoveryValue, TransmissionValue, LabValue);

            return builder.ToString();
        }
    }
}
