using System;

namespace ScienceAlert.VesselContext.Experiments.Triggers
{
    public class TriggerIsBusyException : Exception
    {
        public TriggerIsBusyException(ScienceExperiment experiment)
            : base("Experiment trigger for " + experiment.id + " is busy")
        {

        }
    }
}