using System;
using UnityEngine;

namespace ScienceAlert.VesselContext
{
    public struct ExperimentStatusReport
    {
        public ScienceExperiment Experiment { get; private set; }
        public float CollectionValue { get; private set; }
        public float TransmissionValue { get; private set; }
        public float LabValue { get; private set; }
        public bool Onboard { get; private set; }
        public bool Available { get; private set; }
        public bool Runnable { get; private set; }

        public ExperimentStatusReport(
            ScienceExperiment experiment, 
            float collectionValue, 
            float transmissionValue,
            float labValue, 
            bool onboard, 
            bool available, 
            bool runnable) : this()
        {
            if (experiment == null) throw new ArgumentNullException("experiment");

            Experiment = experiment;
            CollectionValue = collectionValue;
            TransmissionValue = transmissionValue;
            LabValue = labValue;
            Onboard = onboard;
            Available = available;
            Runnable = runnable;
        }


        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            if (!(obj is ExperimentStatusReport)) return false;

            try
            {
                var report = (ExperimentStatusReport) obj;
                return Equals(report);
            }
            catch (Exception)
            {
                return false;
            }
        }


        public bool Equals(ExperimentStatusReport report)
        {
            return ReferenceEquals(Experiment, report.Experiment) &&
                   Mathf.Approximately(CollectionValue, report.CollectionValue) &&
                   Mathf.Approximately(TransmissionValue, report.TransmissionValue) &&
                   Mathf.Approximately(LabValue, report.LabValue) &&
                   Onboard == report.Onboard &&
                   Available == report.Available &&
                   Runnable == report.Runnable;
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
                hash = hash * 479 + Runnable.GetHashCode();

                return hash;
            }
        }
    }
}