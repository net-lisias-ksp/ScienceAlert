//using System;
//using ScienceAlert.VesselContext.Experiments.Sensors.Queries;

//namespace ScienceAlert.VesselContext.Experiments.Sensors
//{
//    public class Sensor<T> : ISensor<T> where T : struct
//    {
//        private readonly IQuerySensorValue<T> _value;
  
//        public Sensor(IQuerySensorValue<T> value)
//        {
//            if (value == null) throw new ArgumentNullException("value");
//            _value = value;
//        }


//        public virtual void Update()
//        {
//            T nextValue = _value.Passes();

//            HasChanged = IsValueDifferent(Value, nextValue);

//            Value = nextValue;
//        }


//        public void ClearChangedFlag()
//        {
//            HasChanged = false;
//        }


//        protected virtual bool IsValueDifferent(T original, T nextValue)
//        {
//            return original.Equals(nextValue);
//        }

//        public bool HasChanged { get; protected set; }

//        public T Value { get; protected set; }
//    }
//}
