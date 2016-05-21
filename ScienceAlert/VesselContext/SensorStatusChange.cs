using ScienceAlert.VesselContext.Experiments;

namespace ScienceAlert.VesselContext
{
    struct SensorStatusChange
    {
        public readonly ExperimentSensorState NewState;
        public readonly ExperimentSensorState OldState;

        public SensorStatusChange(ExperimentSensorState newState, ExperimentSensorState oldState)
            : this()
        {
            NewState = newState;
            OldState = oldState;
        }
    }
}
