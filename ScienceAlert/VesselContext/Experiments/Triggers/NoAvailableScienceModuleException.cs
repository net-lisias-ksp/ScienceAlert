using System;

namespace ScienceAlert.VesselContext.Experiments.Triggers
{
    public class NoAvailableScienceModuleException : Exception
    {
        public NoAvailableScienceModuleException(ScienceExperiment experiment)
            : base("Could not find a suitable science module to deploy " + experiment.id + " with.")
        {

        }
    }
}