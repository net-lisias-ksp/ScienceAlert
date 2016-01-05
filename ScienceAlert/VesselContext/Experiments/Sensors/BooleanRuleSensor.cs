using System;
using ScienceAlert.VesselContext.Experiments.Rules;

namespace ScienceAlert.VesselContext.Experiments.Sensors
{
    public class BooleanRuleSensor : Sensor<bool>, IOnboardSensor, IAvailabilitySensor
    {
        private readonly IExperimentRule _sensorRule;

        public BooleanRuleSensor(IExperimentRule sensorRule, ISensorValueQuery<bool> valueQuery) : base(valueQuery)
        {
            if (sensorRule == null) throw new ArgumentNullException("sensorRule");
            _sensorRule = sensorRule;
        }


        public override void Update()
        {
            base.Update();

            var passesRuleThisFrame = _sensorRule.Get();

            HasChanged |= (passesRuleThisFrame != PassesRuleCheck);
            PassesRuleCheck = passesRuleThisFrame;
        }

        // note: override base class because it will box T and create unnecessary garbage
        protected override bool IsValueDifferent(bool original, bool nextValue)
        {
            return original != nextValue; 
        }

        public virtual bool PassesRuleCheck { get; private set; }
    }
}
