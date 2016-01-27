//using System;
//using System.Collections.Generic;
//using System.Linq;
//using ReeperCommon.Containers;
//using ScienceAlert.Core;
//using ScienceAlert.Game;
//using ScienceAlert.VesselContext.Experiments.Rules;
//using ScienceAlert.VesselContext.Experiments.Sensors.Queries;

//namespace ScienceAlert.VesselContext.Experiments.Sensors
//{
//// ReSharper disable once UnusedMember.Global
//    public class ExperimentSensorFactory : ISensorFactory
//    {
//        private readonly IVessel _currentVessel;
//        private readonly IExperimentRuleFactory _ruleFactory;
//        private readonly IEnumerable<ExperimentRuleset> _rulesets;
//        private readonly IScienceSubjectProvider _queryScienceSubject;
//        private readonly IQueryScienceValue _queryScienceValue;
//        private readonly float _careerScienceGainMultiplier;

//        public ExperimentSensorFactory(
//            IVessel currentVessel,
//            IExperimentRuleFactory ruleFactory, 
//            IEnumerable<ExperimentRuleset> rulesets,
//            IScienceSubjectProvider queryScienceSubject,
//            IQueryScienceValue queryScienceValue,
//            [Name(CoreKeys.CareerScienceGainMultiplier)] float careerScienceGainMultiplier)
//        {
//            if (currentVessel == null) throw new ArgumentNullException("currentVessel");
//            if (ruleFactory == null) throw new ArgumentNullException("ruleFactory");
//            if (rulesets == null) throw new ArgumentNullException("rulesets");
//            if (queryScienceSubject == null) throw new ArgumentNullException("QueryScienceSubject");
//            if (queryScienceValue == null) throw new ArgumentNullException("queryScienceValue");

//            _currentVessel = currentVessel;
//            _ruleFactory = ruleFactory;
//            _rulesets = rulesets;
//            _queryScienceSubject = queryScienceSubject;
//            _queryScienceValue = queryScienceValue;
//            _careerScienceGainMultiplier = careerScienceGainMultiplier;
//        }


//        public IOnboardSensor CreateOnboardSensor(ScienceExperiment experiment)
//        {
//            return
//                new BooleanSensor(
//                    new RuleToQuerySensorAdapter(_ruleFactory.Create(experiment,
//                        GetRuleset(experiment).OnboardDefinition)));
//        }


//        public IAvailabilitySensor CreateAvailabilitySensor(ScienceExperiment experiment)
//        {
//            return new BooleanSensor(
//                new RuleToQuerySensorAdapter(_ruleFactory.Create(experiment, 
//                    GetRuleset(experiment).AvailabilityDefinition)));
//        }


//        public ICollectionSensor CreateCollectionSensor(ScienceExperiment experiment)
//        {
//            return new FloatSensor(
//                new ScienceCollectionValueSensor(
//                    experiment, 
//                    _queryScienceSubject, 
//                    _queryScienceValue, 
//                    _careerScienceGainMultiplier, 
//                    _currentVessel));
//        }

//        // note to self: scientists apparently affect the value of transmission
//        public ITransmissionSensor CreateTransmissionSensor(ScienceExperiment experiment)
//        {
//            return new FloatSensor(new DefaultValue<float>());
//        }


//        public ILabDataSensor CreateLabDataSensor(ScienceExperiment experiment)
//        {
//            return new FloatSensor(new DefaultValue<float>());
//        }


//        private ExperimentRuleset GetRuleset(ScienceExperiment experiment)
//        {
//            if (experiment == null) throw new ArgumentNullException("experiment");

//            return _rulesets.FirstOrDefault(rs => rs.Experiment.id == experiment.id)
//                .IfNull(() => { throw new MissingExperimentRulesetException(experiment); });
//        }
//    }
//}
