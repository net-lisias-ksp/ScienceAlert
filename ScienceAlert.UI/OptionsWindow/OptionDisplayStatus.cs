using System;
using JetBrains.Annotations;

namespace ScienceAlert.UI.OptionsWindow
{
    public struct OptionDisplayStatus
    {
        public OptionDisplayStatus(
            IExperimentIdentifier experiment,
            string experimentTitle,
            bool alertsEnabled,
            bool stopWarp,
            bool recoveryAlerts,
            bool transmissionAlerts,
            bool labAlerts,
            bool soundOnAlert,
            bool animationOnAlert,
            float subjectThreshold,
            float minimumThreshold) : this()
        {
            if (experiment == null) throw new ArgumentNullException("experiment");
            Experiment = experiment;
            ExperimentTitle = string.IsNullOrEmpty(experimentTitle) ? "<unknown experiment>" : experimentTitle;
            AlertsEnabled = alertsEnabled;
            StopWarp = stopWarp;
            RecoveryAlerts = recoveryAlerts;
            TransmissionAlerts = transmissionAlerts;
            LabAlerts = labAlerts;
            SoundOnAlert = soundOnAlert;
            AnimationOnAlert = animationOnAlert;
            SubjectResearchThreshold = subjectThreshold;
            ReportMinimumThreshold = minimumThreshold;
        }

        public IExperimentIdentifier Experiment { get; private set; }
        public string ExperimentTitle { get; private set; }
        public bool AlertsEnabled { get; private set; }
        public bool StopWarp { get; private set; }
        public bool RecoveryAlerts { get; private set; }
        public bool TransmissionAlerts { get; private set; }
        public bool LabAlerts { get; private set; }
        public bool SoundOnAlert { get; private set; }
        public bool AnimationOnAlert { get; private set; }
        public float SubjectResearchThreshold { get; private set; }
        public float ReportMinimumThreshold { get; private set; }
    }
}
