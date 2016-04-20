using System;

namespace ScienceAlert.VesselContext.Experiments.Rules
{
    [DoNotAutoRegister]
    class CompositeRule : IExperimentRule
    {
        private readonly Func<IExperimentRule[], bool> _comparisonFunc;
        protected readonly IExperimentRule[] Rules;

        public CompositeRule(Func<IExperimentRule[], bool> comparisonFunc, params IExperimentRule[] rules)
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
