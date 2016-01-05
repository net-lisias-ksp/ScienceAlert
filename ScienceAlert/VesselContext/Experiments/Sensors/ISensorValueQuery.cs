namespace ScienceAlert.VesselContext.Experiments.Sensors
{
    public interface ISensorValueQuery<out T> where T:struct
    {
        T Get();
    }
}
