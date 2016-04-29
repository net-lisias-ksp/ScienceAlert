using System;

namespace ScienceAlert.SensorDefinitions
{
    public class SensorDefinition : /*ITriggerDefinitionProvider,*/ IRuleDefinitionProvider
    {
        public ScienceExperiment Experiment { get; private set; }

        public ConfigNode OnboardRuleDefinition { get; private set; }
        public ConfigNode AvailabilityRuleDefinition { get; private set; }
        public ConfigNode ConditionRuleDefinition { get; private set; }
        public ConfigNode TriggerDefinition { get; private set; }

        public SensorDefinition(
            ScienceExperiment experiment,
            ConfigNode onboardRuleDef,
            ConfigNode availabilityRuleDef,
            ConfigNode conditionRuleDef,
            ConfigNode triggerDef)
        {
            if (experiment == null) throw new ArgumentNullException("experiment");
            if (onboardRuleDef == null) throw new ArgumentNullException("OnboardRuleFactory");
            if (availabilityRuleDef == null) throw new ArgumentNullException("AvailabilityRuleFactory");
            if (conditionRuleDef == null) throw new ArgumentNullException("ConditionRuleFactory");
            if (triggerDef == null) throw new ArgumentNullException("triggerDef");

            Experiment = experiment;
            OnboardRuleDefinition = onboardRuleDef;
            AvailabilityRuleDefinition = availabilityRuleDef;
            ConditionRuleDefinition = conditionRuleDef;
            TriggerDefinition = triggerDef;
        }


        public override string ToString()
        {
            return typeof (SensorDefinition).Name + " (experiment: " + Experiment.id + ")";
        }
    }
}
