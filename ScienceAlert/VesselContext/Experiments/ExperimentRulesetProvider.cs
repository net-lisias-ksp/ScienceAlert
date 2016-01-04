using System;
using System.Collections.Generic;
using System.Linq;
using ReeperCommon.Extensions;
using ScienceAlert.VesselContext.Experiments.Rules;

namespace ScienceAlert.VesselContext.Experiments
{
// ReSharper disable once ClassNeverInstantiated.Global
    public class ExperimentRulesetProvider : IExperimentRulesetProvider
    {
        private readonly List<ExperimentRuleset> _rulesets;

        public ExperimentRulesetProvider(IEnumerable<ExperimentRuleset> rulesets)
        {
            if (rulesets == null) throw new ArgumentNullException("rulesets");
            _rulesets = rulesets.ToList();
        }

        public ExperimentRuleset GetRuleset(ScienceExperiment experiment)
        {
            try
            {
                var ruleset = _rulesets.SingleOrDefault(r => ReferenceEquals(r.Experiment, experiment));

                if (ruleset.IsNull())
                    throw new NoRulesetDefinedForExperimentException(experiment);

                return ruleset;
            }
            catch (InvalidOperationException)
            {
                throw new ArgumentException("Multiple rulesets for " + experiment.id + " defined");
            }
        }
    }
}
