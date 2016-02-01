using System;
using System.Linq;

namespace ScienceAlert.VesselContext.Experiments.Rules
{
// ReSharper disable once ClassNeverInstantiated.Global
    public class CompositeRule : IExperimentRule
    {
        public enum CompositeType
        {
            All,                // all rules in ruleset must pass
            Any,                // any passing rule passes the ruleset
        }

        private readonly CompositeType _compositeType;
        private readonly IExperimentRule[] _rules;


        public CompositeRule(CompositeType compositeType, params IExperimentRule[] rules)
        {
            if (rules == null) throw new ArgumentNullException("rules");
            _compositeType = compositeType;
            _rules = rules;
        }


        public bool Passes()
        {
            switch (_compositeType)
            {
                case CompositeType.All:
                    return _rules.All(r => r.Passes());

                case CompositeType.Any:
                    return _rules.Any(r => r.Passes());

                default:
                    throw new NotImplementedException("Unknown composite node type: " + _compositeType);
            }
        }
    }
}
