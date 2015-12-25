using ScienceAlert.VesselContext.Experiments.Rules;
using strange.extensions.injector.api;

namespace ScienceAlert.VesselContext.Experiments
{
    public interface IExperimentRuleFactory
    {
        IExperimentRule Create(ScienceExperiment experiment, RuleDefinition definition);
    }
}
