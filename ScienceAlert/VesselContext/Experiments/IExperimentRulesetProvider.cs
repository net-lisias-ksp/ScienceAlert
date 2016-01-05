using ScienceAlert.VesselContext.Experiments.Rules;

namespace ScienceAlert.VesselContext.Experiments
{
    public interface IExperimentRulesetProvider
    {
        ExperimentRuleset GetRuleset(ScienceExperiment experiment);
    }
}
