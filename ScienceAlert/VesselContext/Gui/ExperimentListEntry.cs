using System;
using UnityEngine;

namespace ScienceAlert.VesselContext.Gui
{
    public struct ExperimentListEntry
    {
        public string ExperimentTitle { get; private set; }
        public bool DeployButtonEnabled { get; private set; }
        public bool DisplayInExperimentList { get; private set; }
        public float CollectionValue { get; private set; }
        public bool CollectionAlert { get; private set; }
        public float TransmissionValue { get; private set; }
        public bool TransmissionAlert { get; private set; }
        public float LabValue { get; private set; }
        public bool LabAlert { get; private set; }

        public ExperimentListEntry(
            string experimentTitle,
            bool deployButtonEnabled,
            bool displayInExperimentList,
            float collectionValue,
            bool collectionAlert,
            float transmissionValue,
            bool transmissionAlert,
            float labValue,
            bool labAlert) : this()
        {
            ExperimentTitle = experimentTitle;
            DeployButtonEnabled = deployButtonEnabled;
            DisplayInExperimentList = displayInExperimentList;
            CollectionValue = collectionValue;
            CollectionAlert = collectionAlert;
            TransmissionValue = transmissionValue;
            TransmissionAlert = transmissionAlert;
            LabValue = labValue;
            LabAlert = labAlert;
        }

        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            if (!(obj is ExperimentListEntry)) return false;

            try
            {
                var report = (ExperimentListEntry)obj;
                return Equals(report);
            }
            catch (Exception)
            {
                return false;
            }
        }


        public bool Equals(ExperimentListEntry entry)
        {
            return string.Equals(ExperimentTitle, entry.ExperimentTitle) &&
                   DeployButtonEnabled == entry.DeployButtonEnabled &&
                   Mathf.Approximately(CollectionValue, entry.CollectionValue) &&
                   CollectionAlert == entry.CollectionAlert &&
                   Mathf.Approximately(TransmissionValue, entry.TransmissionValue) &&
                   TransmissionAlert == entry.TransmissionAlert &&
                   Mathf.Approximately(LabValue, entry.LabValue) &&
                   LabAlert == entry.LabAlert;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 317;

                hash = hash * 479 + ExperimentTitle.GetHashCode();
                hash = hash * 479 + DeployButtonEnabled.GetHashCode();
                hash = hash * 479 + CollectionValue.GetHashCode();
                hash = hash * 479 + CollectionAlert.GetHashCode();
                hash = hash * 479 + TransmissionValue.GetHashCode();
                hash = hash * 479 + TransmissionAlert.GetHashCode();
                hash = hash * 479 + LabValue.GetHashCode();
                hash = hash * 479 + LabAlert.GetHashCode();

                return hash;
            }
        }
    }
}