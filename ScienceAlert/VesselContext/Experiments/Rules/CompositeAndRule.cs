using System;
using System.Collections.Generic;
using System.Linq;

namespace ScienceAlert.VesselContext.Experiments.Rules
{
    class CompositeAndRule : IExperimentRule
    {
        private readonly IExperimentRule[] _rules;

        public CompositeAndRule(IEnumerable<IExperimentRule> rules)
        {
            if (rules == null) throw new ArgumentNullException("rules");
            _rules = rules.ToArray();
        }


        public bool Passes()
        {
            return _rules.All(r => r.Passes());
        }
    }
}