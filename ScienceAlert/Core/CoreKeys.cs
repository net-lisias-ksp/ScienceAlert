namespace ScienceAlert.Core
{
    public enum CoreKeys
    {
        CoreContextView, // GameObject

        GameData, // IDirectory

        ExperimentRuleTypes, // IEnumerable<Type>
        CustomRulesets, // IEnumerable<ConfigNode>
        DefaultOnboardRuleDefinition, // ConfigNode
        DefaultAvailabilityRuleDefinition, // ConfigNode
        DefaultConditionRuleDefinition, // ConfigNode

        CareerScienceGainMultiplier, // float
        HomeWorld, // ICelestialBody
    }
}
