using System;

namespace ScienceAlert.UI.ExperimentWindow
{
    public struct ExperimentEntryInfo
    {
        public string ExperimentTitle { get; private set; }
        public float CollectionValue { get; private set; }
        public float TransmissionValue { get; private set; }
        public float LabValue { get; private set; }

        public bool ButtonDisplayed { get; private set; }
        public bool ButtonEnabled { get; private set; }

        // These are the indicators inside the experiment list entry
        public bool CollectionAlertLit { get; private set; }
        public bool TransmissionAlertLit { get; private set; }
        public bool LabAlertLit { get; private set; }


        public ExperimentEntryInfo(
            string title, 
            float collectionValue, 
            bool collectionAlert,
            float transmissionValue, 
            bool transmissionAlert,
            float labValue,
            bool labAlert,
            bool showInList, 
            bool buttonEnabled) : this()
        {
            if (string.IsNullOrEmpty(title)) throw new ArgumentException("cannot be null or empty", "title");
            if (collectionValue < 0f) throw new ArgumentOutOfRangeException("collectionValue", "must be >= 0");
            if (transmissionValue < 0f) throw new ArgumentOutOfRangeException("transmissionValue", "must be >= 0");
            if (labValue < 0f) throw new ArgumentOutOfRangeException("labValue", "must be >= 0");

            ExperimentTitle = title;
            CollectionValue = collectionValue;
            CollectionAlertLit = collectionAlert;
            TransmissionValue = transmissionValue;
            TransmissionAlertLit = transmissionAlert;
            LabValue = labValue;
            LabAlertLit = labAlert;
            ButtonDisplayed = showInList;
            ButtonEnabled = buttonEnabled;
        }
    }
}
