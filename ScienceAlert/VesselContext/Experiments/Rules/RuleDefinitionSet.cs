using System;

namespace ScienceAlert.VesselContext.Experiments.Rules
{
    public class RuleDefinitionSet
    {
        public ScienceExperiment Experiment { get; private set; }

        public ConfigNode OnboardRuleDefinition { get; private set; }
        public ConfigNode AvailabilityRuleDefinition { get; private set; }
        public ConfigNode ConditionRuleDefinition { get; private set; }


        public RuleDefinitionSet(
            ScienceExperiment experiment,
            ConfigNode onboardRuleDefinition,
            ConfigNode availabilityRuleDefinition,
            ConfigNode conditionRuleDefinition)
        {
            if (experiment == null) throw new ArgumentNullException("experiment");
            if (onboardRuleDefinition == null) throw new ArgumentNullException("onboardRuleDefinition");
            if (availabilityRuleDefinition == null) throw new ArgumentNullException("availabilityRuleDefinition");
            if (conditionRuleDefinition == null) throw new ArgumentNullException("conditionRuleDefinition");

            if (!onboardRuleDefinition.HasData)
                throw new ArgumentException("Onboard rule definition contains no data", "onboardRuleDefinition");

            if (!availabilityRuleDefinition.HasData)
                throw new ArgumentException("Availability rule definition contains no data",
                    "availabilityRuleDefinition");

            if (!conditionRuleDefinition.HasData)
                throw new ArgumentException("Condition rule definition contains no data", "conditionRuleDefinition");

            Experiment = experiment;
            OnboardRuleDefinition = onboardRuleDefinition;
            AvailabilityRuleDefinition = availabilityRuleDefinition;
            ConditionRuleDefinition = conditionRuleDefinition;

            Log.Verbose(() => "RuleDefinitionSet created for " + Experiment.id);
        }
    }
}
