using System;
using System.Text;
using UnityEngine;

namespace ScienceAlert.VesselContext
{
    public struct ExperimentSensorState
    {
        public ScienceExperiment Experiment { get; private set; }
        public float CollectionValue { get; private set; }
        public float TransmissionValue { get; private set; }
        public float LabValue { get; private set; }
        public bool Onboard { get; private set; }
        public bool Available { get; private set; }
        public bool ConditionsMet { get; private set; }

        public ExperimentSensorState(
            ScienceExperiment experiment, 
            float collectionValue, 
            float transmissionValue,
            float labValue, 
            bool onboard, 
            bool available,
            bool conditionsMet) : this()
        {
            if (experiment == null) throw new ArgumentNullException("experiment");

            Experiment = experiment;
            CollectionValue = collectionValue;
            TransmissionValue = transmissionValue;
            LabValue = labValue;
            Onboard = onboard;
            Available = available;
            ConditionsMet = conditionsMet;
        }


        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            if (!(obj is ExperimentSensorState)) return false;

            try
            {
                var report = (ExperimentSensorState) obj;
                return Equals(report);
            }
            catch (Exception)
            {
                return false;
            }
        }


        public bool Equals(ExperimentSensorState report)
        {
            return ReferenceEquals(Experiment, report.Experiment) &&
                   Mathf.Approximately(CollectionValue, report.CollectionValue) &&
                   Mathf.Approximately(TransmissionValue, report.TransmissionValue) &&
                   Mathf.Approximately(LabValue, report.LabValue) &&
                   Onboard == report.Onboard &&
                   Available == report.Available &&
                   ConditionsMet == report.ConditionsMet;
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

                return hash;
            }
        }

        public override string ToString()
        {
            var sb = new StringBuilder();

            sb.Append(typeof (ExperimentSensorState).Name);
            sb.Append("[");
            sb.Append(Experiment.id);
            sb.Append("] ");
            sb.AppendFormat("Onboard: {0}, Available: {1}, ConditionsMet: {2}, Collection: {3}, Transmission: {4}, Lab: {5}", Onboard,
                Available, ConditionsMet, CollectionValue, TransmissionValue, LabValue);

            return sb.ToString();
        }
    }
}