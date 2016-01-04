using ScienceAlert.Game;
using strange.extensions.signal.impl;

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
    public class SignalVesselChanged : Signal<IVessel>
    {
    }


// ReSharper disable once ClassNeverInstantiated.Global
    public class SignalVesselModified : Signal<IVessel>
    {
    }


// ReSharper disable once ClassNeverInstantiated.Global
    public class SignalActiveVesselModified : Signal
    {
    }




// ReSharper disable once ClassNeverInstantiated.Global
    public class SignalVesselDestroyed : Signal<IVessel>
    {
    }


// ReSharper disable once ClassNeverInstantiated.Global
    public class SignalActiveVesselDestroyed : Signal
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


// ReSharper disable once ClassNeverInstantiated.Global
    // Dispatched after something went wrong during initialization and we're trying to shut down without breaking anything
    public class SignalCriticalShutdown : Signal
    {
        
    }


    public class SignalSharedConfigurationSaving : Signal
    {
        
    }
}
