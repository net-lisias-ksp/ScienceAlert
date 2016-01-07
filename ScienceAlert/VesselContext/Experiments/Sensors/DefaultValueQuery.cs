namespace ScienceAlert.VesselContext.Experiments.Sensors
{
    public class DefaultValueQuery<T> : ISensorValueQuery<T> where T:struct
    {
        public T Get()
        {
            return default(T);
        }
    }
}
