namespace ScienceAlert.VesselContext.Experiments.Sensors.Queries
{
    public interface IQuerySensorValue<out T> where T:struct
    {
        T Get();
    }
}
