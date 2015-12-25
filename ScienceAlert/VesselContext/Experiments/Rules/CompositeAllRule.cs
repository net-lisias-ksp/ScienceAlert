using System;
using System.Linq;

namespace ScienceAlert.VesselContext.Experiments.Rules
{
    public class CompositeAllRule : IExperimentRule
    {
        private readonly IExperimentRule[] _rules;

        public CompositeAllRule(params IExperimentRule[] rules)
        {
            if (rules == null) throw new ArgumentNullException("rules");
            _rules = rules;
        }


        public bool Get()
        {
            return _rules.All(r => r.Get());
        }
    }
}
