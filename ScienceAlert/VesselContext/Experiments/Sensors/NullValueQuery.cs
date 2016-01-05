namespace ScienceAlert.VesselContext.Experiments.Sensors
{
    public class NullValueQuery<T> : ISensorValueQuery<T> where T:struct
    {
        public T Get()
        {
            return default(T);
        }
    }
}
