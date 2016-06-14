using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JetBrains.Annotations;
using ScienceAlert.Game;
using ScienceAlert.VesselContext.Experiments.Sensors.Rules;
using UnityEngine;

namespace ScienceAlert.VesselContext.Experiments.Sensors
{
    public class DefaultExperimentSensor : IExperimentSensor
    {
        private readonly ScienceExperiment _experiment;
        private readonly IExistingScienceSubjectProvider _scienceSubjectProvider;
        private readonly IVessel _activeVessel;
        private readonly IExperimentReportValueCalculator _reportValueCalculator;
        private readonly ISensorRule _onboardRule;
        private readonly ISensorRule _conditionRule;
        private readonly ISensorRule _availabilityRule;

        public DefaultExperimentSensor([NotNull] ScienceExperiment experiment,
            [NotNull] IExistingScienceSubjectProvider scienceSubjectProvider, 
            [NotNull] IVessel activeVessel, 
            [NotNull] IExperimentReportValueCalculator reportValueCalculator,
            [Name(RuleKeys.Onboard), NotNull] ISensorRule onboardRule, 
            [Name(RuleKeys.Condition), NotNull] ISensorRule conditionRule,
            [Name(RuleKeys.Availability), NotNull] ISensorRule availabilityRule,
            [NotNull] IScienceSubject initialSubject)
        {
            if (experiment == null) throw new ArgumentNullException("experiment");
            if (scienceSubjectProvider == null) throw new ArgumentNullException("scienceSubjectProvider");
            if (activeVessel == null) throw new ArgumentNullException("activeVessel");
            if (reportValueCalculator == null) throw new ArgumentNullException("reportValueCalculator");
            if (onboardRule == null) throw new ArgumentNullException("onboardRule");
            if (conditionRule == null) throw new ArgumentNullException("conditionRule");
            if (availabilityRule == null) throw new ArgumentNullException("availabilityRule");
            if (initialSubject == null) throw new ArgumentNullException("initialSubject");

            _experiment = experiment;
            _scienceSubjectProvider = scienceSubjectProvider;
            _activeVessel = activeVessel;
            _reportValueCalculator = reportValueCalculator;
            _onboardRule = onboardRule;
            _conditionRule = conditionRule;
            _availabilityRule = availabilityRule;

            Subject = initialSubject;
        }

        public bool HasChanged { get; private set; }

        public ScienceExperiment Experiment { get { return _experiment; } } 
        public IScienceSubject Subject { get; private set; }

        public bool ConditionsMet { get; private set; }
        public bool Onboard { get; private set; }
        public bool Available { get; private set; }
        public float RecoveryValue { get; private set; }
        public float TransmissionValue { get; private set; }
        public float LabValue { get; private set; }

        public ExperimentSensorState State
        {
            get
            {
                return new ExperimentSensorState(_experiment, Subject, RecoveryValue, TransmissionValue,
                    LabValue, Onboard, Available, ConditionsMet);
            }
        }

        public void ClearChangedFlag()
        {
            HasChanged = false;
        }


        public void UpdateSensorValues()
        {
            var oldSubject = Subject;
            Subject = _scienceSubjectProvider.GetExistingSubject(Experiment, _activeVessel.ExperimentSituation, _activeVessel.OrbitingBody, _activeVessel.Biome);

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
            RecoveryValue = _reportValueCalculator.CalculateRecoveryValue(Experiment, Subject);
        }


        private void UpdateTransmissionValue()
        {
            TransmissionValue = _reportValueCalculator.CalculateTransmissionValue(Experiment, Subject);
        }


        private void UpdateLabValue()
        {
            LabValue = _reportValueCalculator.CalculateLabValue(Experiment, Subject);
        }


        private void UpdateOnboardValue()
        {
            Onboard = _onboardRule.Passes();
        }


        private void UpdateAvailabilityValue()
        {
            Available = _availabilityRule.Passes();
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

