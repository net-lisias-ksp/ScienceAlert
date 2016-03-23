using strange.extensions.signal.impl;

namespace ScienceAlert.VesselContext
{

// ReSharper disable once ClassNeverInstantiated.Global
    public class SignalExperimentSensorStatusChanged : Signal<ExperimentSensorState>
    {
    }


    public class SignalDeployExperiment : Signal<ScienceExperiment>
    {
    }

    public class SignalDeployExperimentFinished : Signal<ScienceExperiment, bool /* successfully? */>
    {
        
    }
}
