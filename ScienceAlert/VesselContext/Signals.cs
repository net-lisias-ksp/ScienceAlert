using strange.extensions.signal.impl;
// ReSharper disable ClassNeverInstantiated.Global

namespace ScienceAlert.VesselContext
{
    class SignalExperimentSensorStatusChanged : Signal<SensorStatusChange>
    {
    }


    class SignalDeployExperiment : Signal<ScienceExperiment>
    {
    }


    public class SignalDeployExperimentFinished : Signal<ScienceExperiment, bool /* successfully? */>
    {  
    }
}
