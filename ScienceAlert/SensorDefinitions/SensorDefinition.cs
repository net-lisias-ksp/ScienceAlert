using System;

namespace ScienceAlert.SensorDefinitions
{
    // Note: factories are supplied rather than the actual ConfigNode so we can validate its contents right away
    // and to keep all the logic tied to creating definitions in the same context
    public class SensorDefinition
    {
        public ScienceExperiment Experiment { get; private set; }

        public ConfigNode OnboardRuleDefinition { get; private set; }
        public ConfigNode AvailabilityRuleDefinition { get; private set; }
        public ConfigNode ConditionRuleDefinition { get; private set; }

        public SensorDefinition(
            ScienceExperiment experiment,
            ConfigNode onboardRuleDef,
            ConfigNode availabilityRuleDef,
            ConfigNode conditionRuleDef)
        {
            if (experiment == null) throw new ArgumentNullException("experiment");
            if (onboardRuleDef == null) throw new ArgumentNullException("OnboardRuleFactory");
            if (availabilityRuleDef == null) throw new ArgumentNullException("AvailabilityRuleFactory");
            if (conditionRuleDef == null) throw new ArgumentNullException("ConditionRuleFactory");

            Experiment = experiment;
            OnboardRuleDefinition = onboardRuleDef;
            AvailabilityRuleDefinition = availabilityRuleDef;
            ConditionRuleDefinition = conditionRuleDef;
        }


        public override string ToString()
        {
            return typeof (SensorDefinition).Name + " (experiment: " + Experiment.id + ")";
        }
    }
}
