using strange.extensions.signal.impl;
using ScienceAlert.Game;
using ScienceAlert.VesselContext;
using ScienceAlert.VesselContext.Experiments;

// ReSharper disable ClassNeverInstantiated.Global

namespace ScienceAlert
{
    // sent when a context is starting (locally)
    public class SignalStart : Signal
    {
    }


    // sent when a signal is being destroyed
    public class SignalContextIsBeingDestroyed : Signal 
    {

    }


    public class SignalActiveVesselChanged : Signal
    {
    }


    public class SignalActiveVesselModified : Signal
    {
    }


    public class SignalActiveVesselDestroyed : Signal
    {
    }
 

    public class SignalCrewOnEva : Signal<GameEvents.FromToAction<IPart, IPart>>
    {
    }


    public class SignalCrewTransferred : Signal<GameEvents.HostedFromToAction<ProtoCrewMember, IPart>>
    {
    }



    public class SignalGameSceneLoadRequested : Signal<GameScenes>
    {
    }


    public class SignalApplicationQuit : Signal
    {
    }


    public class SignalScienceReceived : Signal<float, ScienceSubject, ProtoVessel, bool>
    {
    }


    // Update tick
    public class SignalGameTick : Signal
    {
    }


    public class SignalSharedConfigurationSaving : Signal
    {  
    }


    public class SignalExperimentAlertChanged : Signal<SensorStatusChange, AlertStatusChange>
    {
    }
}
