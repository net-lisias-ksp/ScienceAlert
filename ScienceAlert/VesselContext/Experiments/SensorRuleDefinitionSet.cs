using System;

namespace ScienceAlert.VesselContext.Experiments
{
    public class SensorRuleDefinitionSet
    {
        public ConfigNode OnboardDefinition { get; set; }
        public ConfigNode AvailabilityDefinition { get; set; }
        public ConfigNode ConditionDefinition { get; set; }

        public SensorRuleDefinitionSet(
            ConfigNode onboardDefinition,
            ConfigNode availabilityDefinition,
            ConfigNode conditionDefinition)
        {
            if (onboardDefinition == null) throw new ArgumentNullException("onboardDefinition");
            if (availabilityDefinition == null) throw new ArgumentNullException("availabilityDefinition");
            if (conditionDefinition == null) throw new ArgumentNullException("conditionDefinition");

            OnboardDefinition = onboardDefinition;
            AvailabilityDefinition = availabilityDefinition;
            ConditionDefinition = conditionDefinition;
        }
    }
}