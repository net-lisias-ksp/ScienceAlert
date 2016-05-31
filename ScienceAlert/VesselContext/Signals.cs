using strange.extensions.signal.impl;
using ScienceAlert.VesselContext.Experiments;

// ReSharper disable ClassNeverInstantiated.Global

namespace ScienceAlert.VesselContext
{
    public class SignalExperimentSensorStatusChanged : Signal<SensorStatusChange>
    {
    }


    class SignalDeployExperiment : Signal<ScienceExperiment>
    {
    }


    public class SignalDeployExperimentFinished : Signal<ScienceExperiment, bool /* successfully? */>
    {  
    }
}
