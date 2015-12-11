using System;

namespace ScienceAlert.Rules
{
    public class ExperimentRuleset
    {
        public ScienceExperiment Experiment { get; private set; }
        public RuleDefinition AvailabilityDefinition { get; private set; }
        public RuleDefinition OnboardDefinition { get; private set; }
        

        public ExperimentRuleset(
            ScienceExperiment experiment, 
            RuleDefinition onboard,
            RuleDefinition availability)
        {
            if (experiment == null) throw new ArgumentNullException("experiment");
            if (availability == null) throw new ArgumentNullException("availability");
            if (onboard == null) throw new ArgumentNullException("onboard");

            Experiment = experiment;
            AvailabilityDefinition = availability;
            OnboardDefinition = onboard;
        }
    }
}
