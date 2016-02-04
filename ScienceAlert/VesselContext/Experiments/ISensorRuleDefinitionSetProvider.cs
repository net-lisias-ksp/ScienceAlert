using ReeperCommon.Containers;

namespace ScienceAlert.VesselContext.Experiments
{
    public interface ISensorRuleDefinitionSetProvider
    {
        Maybe<SensorRuleDefinitionSet> GetDefinitionSet(ScienceExperiment experiment);
        SensorRuleDefinitionSet GetDefaultDefinition();
    }
}
