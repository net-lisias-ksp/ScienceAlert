using ScienceAlert.Game;

namespace ScienceAlert.VesselContext.Experiments
{
    public interface IExperimentSensor
    {
        bool HasChanged { get; }
        void UpdateSensorValues();
        void ClearChangedFlag();

        IExperimentSensorState State { get; }
        ScienceExperiment Experiment { get; }
        IScienceSubject Subject { get; }
    }
}