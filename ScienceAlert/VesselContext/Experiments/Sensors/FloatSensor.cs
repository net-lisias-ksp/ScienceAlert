using UnityEngine;

namespace ScienceAlert.VesselContext.Experiments.Sensors
{
    public class FloatSensor : Sensor<float>, ICollectionSensor, ITransmissionSensor, ILabDataSensor
    {
        public FloatSensor(ISensorValueQuery<float> valueQuery) : base(valueQuery)
        {
        }

        protected override bool IsValueDifferent(float original, float nextValue)
        {
            return !Mathf.Approximately(original, nextValue);
        }
    }
}
