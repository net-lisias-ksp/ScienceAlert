using System;

namespace ScienceAlert.VesselContext.Experiments.Rules
{
    public class RulesetMissingExperimentIdException : Exception
    {
        public RulesetMissingExperimentIdException() : base("Rule Definition MUST specify ExperimentID")
        {
            
        }
    }
}
