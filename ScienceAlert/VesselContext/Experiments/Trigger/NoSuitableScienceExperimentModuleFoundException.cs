using System;

namespace ScienceAlert.VesselContext.Experiments.Trigger
{
    class NoSuitableScienceExperimentModuleFoundException : Exception
    {
        public NoSuitableScienceExperimentModuleFoundException(ScienceExperiment experiment)
            : base("No suitable science experiment module for '" + experiment.id + "' was found")
        {
            
        }
    }
}