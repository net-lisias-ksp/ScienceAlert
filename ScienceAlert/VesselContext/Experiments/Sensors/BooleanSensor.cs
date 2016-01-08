using ScienceAlert.VesselContext.Experiments.Sensors.Queries;

namespace ScienceAlert.VesselContext.Experiments.Sensors
{
    public class BooleanSensor : Sensor<bool>, IOnboardSensor, IAvailabilitySensor
    {
        public BooleanSensor(IQuerySensorValue<bool> value) : base(value)
        {
        }

        protected override bool IsValueDifferent(bool original, bool nextValue)
        {
            return original != nextValue;
        }
    }
}
