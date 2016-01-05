namespace ScienceAlert.VesselContext.Experiments.Sensors
{
    public interface ISensor
    {
        void Update();
        void ClearChangedFlag();
        bool HasChanged { get; }
    }
}
