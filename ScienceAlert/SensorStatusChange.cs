using System;
using ScienceAlert.VesselContext.Experiments;

namespace ScienceAlert
{
    public struct SensorStatusChange
    {
        public readonly ExperimentSensorState CurrentState;
        public readonly ExperimentSensorState PreviousState;

        public SensorStatusChange(ExperimentSensorState currentState, ExperimentSensorState previousState)
            : this()
        {
            if (currentState.Experiment.id != previousState.Experiment.id)
                throw new InvalidOperationException("Supplied states are for two different experiments");

            CurrentState = currentState;
            PreviousState = previousState;
        }
    }
}
