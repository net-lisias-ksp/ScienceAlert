using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ReeperCommon.Containers;
using ScienceAlert.Rules;

namespace ScienceAlert.VesselContext.Experiments
{
    public class FailedToCreateRuleException : Exception
    {
        public FailedToCreateRuleException() : base("Could not construct the specified rule")
        {
            
        }

        public FailedToCreateRuleException(RuleDefinition definition)
            : base(
                "Failed to construct rule of type " + definition.Rule.Return(r => r.FullName, "(no rule type defined)"))
        {
            
        }

        public FailedToCreateRuleException(string message, Exception inner) : base(message, inner)
        {
            
        }
    }
}
