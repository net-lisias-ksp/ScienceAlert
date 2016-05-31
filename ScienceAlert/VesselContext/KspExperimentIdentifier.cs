using System;
using JetBrains.Annotations;
using ScienceAlert.UI;

namespace ScienceAlert.VesselContext
{
    class KspExperimentIdentifier : IExperimentIdentifier
    {
        public readonly ScienceExperiment Experiment;

        public KspExperimentIdentifier([NotNull] ScienceExperiment experiment)
        {
            if (experiment == null) throw new ArgumentNullException("experiment");
            Experiment = experiment;
        }

        public override int GetHashCode()
        {
            return Experiment.id.GetHashCode();
        }

        public bool Equals(ScienceExperiment other)
        {
            if (other == null) return false;

            return Experiment.id == other.id;
        }

        public bool Equals(string other)
        {
            if (string.IsNullOrEmpty(other)) return false;

            return Experiment.id == other;
        }

        public bool Equals(IExperimentIdentifier other)
        {
            return other != null && other.Equals(Experiment.id);
        }

        public override bool Equals(object obj)
        {
            var other = obj as KspExperimentIdentifier;

            if (other == null) return false;

            return ReferenceEquals(this, other) || Experiment.id == other.Experiment.id;
        }

        public override string ToString()
        {
            return "KspExperimentIdentifier: " + Experiment.id;
        }
    }
}
