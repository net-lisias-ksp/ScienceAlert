using System;

namespace ScienceAlert.VesselContext.Experiments
{
    public class MissingExperimentException : Exception
    {
        public MissingExperimentException(ScienceExperiment experiment)
            : base("Missing experiment: " + experiment.id + "; something is hosed")
        {

        }
    }
}
