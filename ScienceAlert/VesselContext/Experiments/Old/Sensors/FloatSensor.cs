//using ScienceAlert.VesselContext.Experiments.Sensors.Queries;
//using UnityEngine;

//namespace ScienceAlert.VesselContext.Experiments.Sensors
//{
//    public class FloatSensor : Sensor<float>, ICollectionSensor, ITransmissionSensor, ILabDataSensor
//    {
//        public FloatSensor(IQuerySensorValue<float> value) : base(value)
//        {
//        }

//        protected override bool IsValueDifferent(float original, float nextValue)
//        {
//            return !Mathf.Approximately(original, nextValue);
//        }
//    }
//}
