using System;
using System.Text;
using JetBrains.Annotations;
using ScienceAlert.Game;
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
        public IScienceSubject Subject { get; private set; }

        public ExperimentSensorState(
            ScienceExperiment experiment, [NotNull] IScienceSubject subject,
            float collectionValue, 
            float transmissionValue,
            float labValue, 
            bool onboard, 
            bool available,
            bool conditionsMet) : this()
        {
            if (experiment == null) throw new ArgumentNullException("experiment");
            if (subject == null) throw new ArgumentNullException("subject");

            Experiment = experiment;
            Subject = subject;
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
                hash = hash * 479 + Subject.Id.GetHashCode();

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
            sb.AppendFormat("Subject: {0}, Onboard: {1}, Available: {2}, ConditionsMet: {3}, Collection: {4}, Transmission: {5}, Lab: {6}", Subject.Id, Onboard,
                Available, ConditionsMet, CollectionValue, TransmissionValue, LabValue);

            return sb.ToString();
        }
    }
}