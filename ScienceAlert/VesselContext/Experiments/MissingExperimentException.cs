using System;
using ReeperCommon.Containers;

namespace ScienceAlert.VesselContext.Experiments
{
    public class MissingExperimentException : Exception
    {
        public MissingExperimentException(ScienceExperiment experiment)
            : base("Missing experiment: " + experiment.Return(e => e.id, "<null>") + "; something is hosed")
        {

        }

        public MissingExperimentException(ScienceExperiment experiment, Exception inner)
            : base("Missing experiment: " + experiment.Return(e => e.id, "<null>"), inner)
        {
            
        }
    }
}
