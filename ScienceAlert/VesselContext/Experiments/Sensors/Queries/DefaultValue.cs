namespace ScienceAlert.VesselContext.Experiments.Sensors.Queries
{
    public class DefaultValue<T> : IQuerySensorValue<T> where T:struct
    {
        public T Get()
        {
            return default(T);
        }
    }
}
