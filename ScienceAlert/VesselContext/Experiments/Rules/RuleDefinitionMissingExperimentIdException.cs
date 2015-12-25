using System;

namespace ScienceAlert.VesselContext.Experiments.Rules
{
    public class RuleDefinitionMissingExperimentIdException : Exception
    {
        public RuleDefinitionMissingExperimentIdException() : base("Rule Definition MUST specify experimentID")
        {
            
        }
    }
}
