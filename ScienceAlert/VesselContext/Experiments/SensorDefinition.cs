using System;
using ScienceAlert.VesselContext.Experiments.Rules;

namespace ScienceAlert.VesselContext.Experiments
{
    // Note: factories are supplied rather than the actual ConfigNode so we can validate its contents right away
    // and to keep all the logic tied to creating definitions in the same context
    public class SensorDefinition
    {
        public ScienceExperiment Experiment { get; set; }

        public IRuleFactory OnboardRuleFactory { get; set; }
        public IRuleFactory AvailabilityRuleFactory { get; set; }
        public IRuleFactory ConditionRuleFactory { get; set; }

        public SensorDefinition(
            ScienceExperiment experiment, 
            IRuleFactory onboardRuleFactory,
            IRuleFactory availabilityRuleFactory, 
            IRuleFactory conditionRuleFactory)
        {
            if (experiment == null) throw new ArgumentNullException("experiment");
            if (onboardRuleFactory == null) throw new ArgumentNullException("OnboardRuleFactory");
            if (availabilityRuleFactory == null) throw new ArgumentNullException("AvailabilityRuleFactory");
            if (conditionRuleFactory == null) throw new ArgumentNullException("ConditionRuleFactory");

            Experiment = experiment;
            OnboardRuleFactory = onboardRuleFactory;
            AvailabilityRuleFactory = availabilityRuleFactory;
            ConditionRuleFactory = conditionRuleFactory;
        }


        public override string ToString()
        {
            return typeof (SensorDefinition).Name + " (experiment: " + Experiment.id + ")";
        }
    }
}
