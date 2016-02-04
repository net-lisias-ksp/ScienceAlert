using System;

namespace ScienceAlert.VesselContext.Experiments
{
    public class SensorRulePackageDefinitionNotFoundException : Exception
    {
        public SensorRulePackageDefinitionNotFoundException(ScienceExperiment experiment)
            : base("No sensor rule definition package found for " + experiment.id)
        {
            
        }
    }
}
