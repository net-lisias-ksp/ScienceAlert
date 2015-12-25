namespace ScienceAlert.VesselContext.Experiments
{
    public interface ISensor
    {
        void Poll();
        void UpdateOnboardStatus();
    }
}
