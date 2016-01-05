using System;
using System.Collections.Generic;
using System.Linq;
using ReeperCommon.Containers;
using ScienceAlert.VesselContext.Experiments.Rules;

namespace ScienceAlert.VesselContext.Experiments.Sensors
{
    [Implements(typeof(ISensorFactory))]
// ReSharper disable once UnusedMember.Global
    public class ExperimentSensorFactory : ISensorFactory
    {
        private readonly IExperimentRuleFactory _ruleFactory;
        private readonly IEnumerable<ExperimentRuleset> _rulesets;

        public ExperimentSensorFactory(IExperimentRuleFactory ruleFactory, IEnumerable<ExperimentRuleset> rulesets)
        {
            if (ruleFactory == null) throw new ArgumentNullException("ruleFactory");
            if (rulesets == null) throw new ArgumentNullException("rulesets");
            _ruleFactory = ruleFactory;
            _rulesets = rulesets;
        }

        public IOnboardSensor CreateOnboardSensor(ScienceExperiment experiment)
        {
            return new BooleanRuleSensor(_ruleFactory.Create(experiment, GetRuleset(experiment).OnboardDefinition),
                new NullValueQuery<bool>());
        }

        public IAvailabilitySensor CreateAvailabilitySensor(ScienceExperiment experiment)
        {
            return new BooleanRuleSensor(_ruleFactory.Create(experiment, GetRuleset(experiment).AvailabilityDefinition),
                new NullValueQuery<bool>());
        }

        public ICollectionSensor CreateCollectionSensor(ScienceExperiment experiment)
        {
            return new FloatSensor(new NullValueQuery<float>());
        }

        public ITransmissionSensor CreateTransmissionSensor(ScienceExperiment experiment)
        {
            return new FloatSensor(new NullValueQuery<float>());
        }

        public ILabDataSensor CreateLabDataSensor(ScienceExperiment experiment)
        {
            return new FloatSensor(new NullValueQuery<float>());
        }

        private ExperimentRuleset GetRuleset(ScienceExperiment experiment)
        {
            if (experiment == null) throw new ArgumentNullException("experiment");

            return _rulesets.FirstOrDefault(rs => rs.Experiment.id == experiment.id)
                .IfNull(() => { throw new MissingExperimentRulesetException(experiment); });
        }
    }
}
