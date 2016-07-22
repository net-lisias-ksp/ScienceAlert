using System;

namespace ScienceAlert.UI.ExperimentWindow
{
    public struct EntryDisplayStatus
    {
        public string ExperimentTitle { get; private set; }
        public float RecoveryValue { get; private set; }
        public float TransmissionValue { get; private set; }
        public float LabValue { get; private set; }

        public bool ButtonDisplayed { get; private set; }
        public bool ButtonEnabled { get; private set; }

        // These are the indicators inside the experiment list entry
        public bool AlertLit { get; private set; }
        public bool RecoveryLit { get; private set; }
        public bool TransmissionLit { get; private set; }
        public bool LabLit { get; private set; }


        public EntryDisplayStatus(
            string title, 
            bool alert,
            float recoveryValue, 
            bool recoveryIndicator,
            float transmissionValue, 
            bool transmissionIndicator,
            float labValue,
            bool labIndicator,
            bool showInList, 
            bool buttonEnabled) : this()
        {
            if (string.IsNullOrEmpty(title)) throw new ArgumentException("cannot be null or empty", "title");
            if (recoveryValue < 0f) throw new ArgumentOutOfRangeException("recoveryValue", "must be >= 0");
            if (transmissionValue < 0f) throw new ArgumentOutOfRangeException("transmissionValue", "must be >= 0");
            if (labValue < 0f) throw new ArgumentOutOfRangeException("labValue", "must be >= 0");

            ExperimentTitle = title;
            AlertLit = alert;
            RecoveryValue = recoveryValue;
            RecoveryLit = recoveryIndicator;
            TransmissionValue = transmissionValue;
            TransmissionLit = transmissionIndicator;
            LabValue = labValue;
            LabLit = labIndicator;
            ButtonDisplayed = showInList;
            ButtonEnabled = buttonEnabled;
        }
    }
}
