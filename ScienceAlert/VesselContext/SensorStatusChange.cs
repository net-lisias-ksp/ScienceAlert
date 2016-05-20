using System;
using JetBrains.Annotations;
using ScienceAlert.VesselContext.Experiments;

namespace ScienceAlert.VesselContext
{
    struct SensorStatusChange
    {
        public readonly IExperimentSensorState NewState;
        public readonly IExperimentSensorState OldState;

        public SensorStatusChange([NotNull] IExperimentSensorState newState, [NotNull] IExperimentSensorState oldState)
            : this()
        {
            if (newState == null) throw new ArgumentNullException("newState");
            if (oldState == null) throw new ArgumentNullException("oldState");

            NewState = newState;
            OldState = oldState;
        }
    }
}
