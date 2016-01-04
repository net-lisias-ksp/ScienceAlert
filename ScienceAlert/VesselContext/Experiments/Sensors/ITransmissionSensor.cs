namespace ScienceAlert.VesselContext.Experiments.Sensors
{
    public interface ITransmissionSensor : ISensor
    {
        float Value { get; }
    }
}
