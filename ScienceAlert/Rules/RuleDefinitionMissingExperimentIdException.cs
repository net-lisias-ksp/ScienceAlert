using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ScienceAlert.Rules
{
    public class RuleDefinitionMissingExperimentIdException : Exception
    {
        public RuleDefinitionMissingExperimentIdException() : base("Rule Definition MUST specify experimentID")
        {
            
        }
    }
}
