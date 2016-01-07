using System;

namespace ScienceAlert.VesselContext.Experiments.Sensors
{
    public class Sensor<T> : ISensor<T> where T : struct
    {
        private readonly ISensorValueQuery<T> _valueQuery;
  
        public Sensor(ISensorValueQuery<T> valueQuery)
        {
            if (valueQuery == null) throw new ArgumentNullException("valueQuery");
            _valueQuery = valueQuery;
        }


        public virtual void Update()
        {
            T nextValue = _valueQuery.Get();

            HasChanged = IsValueDifferent(Value, nextValue);

            Value = nextValue;
        }


        public void ClearChangedFlag()
        {
            HasChanged = false;
        }


        protected virtual bool IsValueDifferent(T original, T nextValue)
        {
            return original.Equals(nextValue);
        }

        public bool HasChanged { get; protected set; }

        public T Value { get; protected set; }
    }
}
