using System;
using ScienceAlert.VesselContext.Experiments.Rules;

namespace ScienceAlert.VesselContext.Experiments.Sensors
{
    public class RuleToBooleanSensorQueryAdapter : ISensorValueQuery<bool>
    {
        private readonly IExperimentRule _rule;

        public RuleToBooleanSensorQueryAdapter(
            IExperimentRule rule)
        {
            if (rule == null) throw new ArgumentNullException("rule");
            _rule = rule;
        }

        public bool Get()
        {
            return _rule.Get();
        }
    }
}
