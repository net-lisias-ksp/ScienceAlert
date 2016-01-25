//using System;
//using ScienceAlert.VesselContext.Experiments.Rules;

//namespace ScienceAlert.VesselContext.Experiments.Sensors.Queries
//{
//    public class RuleToQuerySensorAdapter : IQuerySensorValue<bool>
//    {
//        private readonly IExperimentRule _rule;

//        public RuleToQuerySensorAdapter(
//            IExperimentRule rule)
//        {
//            if (rule == null) throw new ArgumentNullException("rule");
//            _rule = rule;
//        }

//        public bool Passes()
//        {
//            return _rule.Passes();
//        }
//    }
//}
