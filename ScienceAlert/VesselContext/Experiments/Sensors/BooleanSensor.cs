namespace ScienceAlert.VesselContext.Experiments.Sensors
{
    public class BooleanSensor : Sensor<bool>, IOnboardSensor, IAvailabilitySensor
    {
        public BooleanSensor(ISensorValueQuery<bool> valueQuery) : base(valueQuery)
        {
        }

        protected override bool IsValueDifferent(bool original, bool nextValue)
        {
            return original != nextValue;
        }
    }
}
