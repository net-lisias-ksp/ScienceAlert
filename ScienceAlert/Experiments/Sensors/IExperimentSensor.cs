using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ScienceAlert.Experiments.Sensors
{
    [Flags]
    public enum SensorState
    {
        NoAlert = 0,                                            
        Recoverable = 1 << 0,                                   
        Transmittable = 1 << 1,                                 

        Both = Recoverable | Transmittable
    }

    interface IExperimentSensor
    {
        float RecoveryValue { get; }
        float TransmissionValue { get; }
        string Subject { get; }
        ProfileData.SensorSettings Settings { get; }
        SensorState Status { get; }
        ScienceExperiment Experiment { get; }

        void UpdateState(CelestialBody body, ExperimentSituations situation);
        bool DeployThisExperiment();
    }
}
