using ReeperCommon.Containers;

namespace ScienceAlert.VesselContext.Experiments.Rules
{
    public interface IRuleBuilderProvider
    {
        Maybe<IRuleBuilder> GetBuilder(ConfigNode config);
    }
}