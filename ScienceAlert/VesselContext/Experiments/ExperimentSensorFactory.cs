using System;

namespace ScienceAlert.VesselContext.Experiments
{
    public class ExperimentSensorFactory : IExperimentSensorFactory
    {
        private readonly IExperimentRulesetProvider _rulesetProvider;
        private readonly IExperimentRuleFactory _ruleFactory;
        private readonly SignalActiveVesselModified _vesselModifiedSignal;
 
        public ExperimentSensorFactory(
            IExperimentRulesetProvider rulesetProvider, 
            IExperimentRuleFactory ruleFactory,
            SignalActiveVesselModified vesselModifiedSignal)
        {
            if (rulesetProvider == null) throw new ArgumentNullException("rulesetProvider");
            if (ruleFactory == null) throw new ArgumentNullException("ruleFactory");
            if (vesselModifiedSignal == null) throw new ArgumentNullException("vesselModifiedSignal");
            _rulesetProvider = rulesetProvider;
            _ruleFactory = ruleFactory;
            _vesselModifiedSignal = vesselModifiedSignal;
        }
        

        public ISensor Create(ScienceExperiment experiment)
        {
            if (experiment == null) throw new ArgumentNullException("experiment");

            var ruleset = _rulesetProvider.GetRuleset(experiment);

            var sensor = new Sensor(experiment, 
                _ruleFactory.Create(experiment, ruleset.OnboardDefinition),
                _ruleFactory.Create(experiment, ruleset.AvailabilityDefinition));

            return sensor;
        }
    }
}
