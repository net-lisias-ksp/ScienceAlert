//using System;
//using ReeperCommon.Serialization;
//using ScienceAlert.Game;
//using ScienceAlert.VesselContext.Experiments.Rules.idea;
//using strange.extensions.injector.api;

//namespace ScienceAlert.VesselContext.Experiments
//{
//    public class SensorFactory
//    {
//        private readonly ScienceExperiment _experiment;
//        private readonly IRuleFactory _onboardRuleFactory;
//        private readonly IRuleFactory _availabilityRuleFactory;
//        private readonly IRuleFactory _conditionRuleFactory;

//        public SensorFactory(
//            ScienceExperiment experiment, 
//            IRuleFactory onboardRuleFactory,
//            IRuleFactory availabilityRuleFactory, 
//            IRuleFactory conditionRuleFactory)
//        {
//            if (experiment == null) throw new ArgumentNullException("experiment");
//            if (onboardRuleFactory == null) throw new ArgumentNullException("onboardRuleFactory");
//            if (availabilityRuleFactory == null) throw new ArgumentNullException("availabilityRuleFactory");
//            if (conditionRuleFactory == null) throw new ArgumentNullException("conditionRuleFactory");
//            _experiment = experiment;
//            _onboardRuleFactory = onboardRuleFactory;
//            _availabilityRuleFactory = availabilityRuleFactory;
//            _conditionRuleFactory = conditionRuleFactory;
//        }


//        public ExperimentSensor Build(
//            IScienceSubjectProvider subjectProvider, 
//            IExperimentReportValueCalculator reportCalculator, 
//            IInjectionBinder context,
//            IConfigNodeSerializer serializer)
//        {
//            if (subjectProvider == null) throw new ArgumentNullException("subjectProvider");
//            if (reportCalculator == null) throw new ArgumentNullException("reportCalculator");
//            if (context == null) throw new ArgumentNullException("context");
//            if (serializer == null) throw new ArgumentNullException("serializer");

//            try
//            {
//                context.Bind<ScienceExperiment>().ToValue(_experiment);

//                return new ExperimentSensor(_experiment, subjectProvider, reportCalculator,
//                    _onboardRuleFactory.Build(context, serializer),
//                    _availabilityRuleFactory.Build(context, serializer),
//                    _conditionRuleFactory.Build(context, serializer));
//            }
//            finally
//            {
//                context.Unbind<ScienceExperiment>();
//            }
//        }
//    }
//}
