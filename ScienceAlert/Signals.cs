using strange.extensions.signal.impl;
using ScienceAlert.Game;

namespace ScienceAlert
{
// ReSharper disable once ClassNeverInstantiated.Global
    public class SignalStart : Signal
    {
    }


// ReSharper disable once ClassNeverInstantiated.Global
    public class SignalContextDestruction : Signal // signal to destroy the current context
    {

    }


// ReSharper disable once ClassNeverInstantiated.Global
    public class SignalActiveVesselChanged : Signal
    {
    }


    // ReSharper disable once ClassNeverInstantiated.Global
    public class SignalActiveVesselModified : Signal
    {
    }


    // ReSharper disable once ClassNeverInstantiated.Global
    public class SignalActiveVesselDestroyed : Signal
    {
    }
 

    public class SignalCrewOnEva : Signal<GameEvents.FromToAction<IPart, IPart>>
    {
        
    }


    public class SignalCrewTransferred : Signal<GameEvents.HostedFromToAction<ProtoCrewMember, IPart>>
    {
        
    }












// ReSharper disable once UnusedMember.Global
    public class SignalGameSceneLoadRequested : Signal<GameScenes>
    {
    }


// ReSharper disable once UnusedMember.Global
    public class SignalApplicationQuit : Signal
    {
    }


    public class SignalScienceReceived : Signal<float, ScienceSubject, ProtoVessel, bool>
    {
    
    }


// ReSharper disable once ClassNeverInstantiated.Global
    public class SignalGameTick : Signal
    {

    }



    public class SignalSharedConfigurationSaving : Signal
    {
        
    }



}
