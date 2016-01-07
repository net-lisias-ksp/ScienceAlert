namespace ScienceAlert.VesselContext.Experiments.Sensors
{
    public interface ISensor
    {
        void Update();
        void ClearChangedFlag();
        bool HasChanged { get; }
    }

    public interface ISensor<T> : ISensor where T : struct
    {
        T Value { get; }
    }
}
