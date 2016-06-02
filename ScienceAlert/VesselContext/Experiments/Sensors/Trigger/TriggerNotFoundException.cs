using System;

namespace ScienceAlert.VesselContext.Experiments.Sensors.Trigger
{
    public class TriggerNotFoundException : Exception
    {
        public TriggerNotFoundException(ScienceExperiment experiment)
            : base("No trigger for '" + experiment.id + "' was found")
        {
        }
    }
}