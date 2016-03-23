namespace ScienceAlert.SensorDefinitions
{
    public interface IRuleDefinitionProvider
    {
        ConfigNode OnboardRuleDefinition { get; }
        ConfigNode AvailabilityRuleDefinition { get; }
        ConfigNode ConditionRuleDefinition { get; }
    }
}