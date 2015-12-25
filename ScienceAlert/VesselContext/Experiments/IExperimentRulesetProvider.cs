using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ScienceAlert.VesselContext.Experiments.Rules;

namespace ScienceAlert.VesselContext.Experiments
{
    public interface IExperimentRulesetProvider
    {
        ExperimentRuleset GetRuleset(ScienceExperiment experiment);
    }
}
