namespace ScienceAlert.VesselContext.Experiments.Sensors
{
    public interface ILabDataSensor : ISensor
    {
        float Value { get; }
    }
}
