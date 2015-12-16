using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ScienceAlert.VesselContext.Experiments
{
    public class NoRulesetDefinedForExperimentException : Exception
    {
        public NoRulesetDefinedForExperimentException() : base("No ruleset defined for specified ScienceExperiment")
        {
            
        }

        public NoRulesetDefinedForExperimentException(ScienceExperiment experiment)
            : base("No ruleset defined for " + experiment.id)
        {
            
        }

        public NoRulesetDefinedForExperimentException(string message, Exception inner) : base(message, inner)
        {
            
        }
    }
}
