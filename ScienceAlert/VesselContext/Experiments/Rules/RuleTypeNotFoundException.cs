using System;

namespace ScienceAlert.VesselContext.Experiments.Rules
{
    public class RuleTypeNotFoundException : Exception
    {
        public RuleTypeNotFoundException() : base("A rule with the given name was not found")
        {
            
        }

        public RuleTypeNotFoundException(string ruleName) : base("Failed to find a rule with name \"" + ruleName + "\"")
        {
            
        }

        public RuleTypeNotFoundException(string message, Exception inner) : base(message, inner)
        {
            
        }
    }
}
