using System;

namespace ScienceAlert.VesselContext.Experiments
{
// ReSharper disable once ClassNeverInstantiated.Global
    public class ExperimentSensorFactory : IExperimentSensorFactory
    {
        private readonly IExperimentRulesetProvider _rulesetProvider;
        private readonly IExperimentRuleFactory _ruleFactory;
        private readonly SignalSensorStateChanged _sensorChangedSignal;

        public ExperimentSensorFactory(
            IExperimentRulesetProvider rulesetProvider, 
            IExperimentRuleFactory ruleFactory,
            SignalSensorStateChanged sensorChangedSignal)
        {
            if (rulesetProvider == null) throw new ArgumentNullException("rulesetProvider");
            if (ruleFactory == null) throw new ArgumentNullException("ruleFactory");
            if (sensorChangedSignal == null) throw new ArgumentNullException("sensorChangedSignal");

            _rulesetProvider = rulesetProvider;
            _ruleFactory = ruleFactory;
            _sensorChangedSignal = sensorChangedSignal;
        }
        

        public ISensor Create(ScienceExperiment experiment)
        {
            if (experiment == null) throw new ArgumentNullException("experiment");

            var ruleset = _rulesetProvider.GetRuleset(experiment);

            var sensor = new Sensor(experiment, 
                _ruleFactory.Create(experiment, ruleset.OnboardDefinition),
                _ruleFactory.Create(experiment, ruleset.AvailabilityDefinition),
                _sensorChangedSignal);

            return sensor;
        }
    }
}
