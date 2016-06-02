using System;

namespace ScienceAlert.VesselContext.Experiments.Sensors.Rules
{
    [DoNotAutoRegister]
    class CompositeRule : ISensorRule
    {
        private readonly Func<ISensorRule[], bool> _comparisonFunc;
        protected readonly ISensorRule[] Rules;

        public CompositeRule(Func<ISensorRule[], bool> comparisonFunc, params ISensorRule[] rules)
        {
            if (comparisonFunc == null) throw new ArgumentNullException("comparisonFunc");
            if (rules == null) throw new ArgumentNullException("rules");
            _comparisonFunc = comparisonFunc;
            Rules = rules;
        }


        public bool Passes()
        {
            return _comparisonFunc(Rules);
        }
    }
}
