using ScienceAlert.Game;
using strange.extensions.signal.impl;

namespace ScienceAlert
{
// ReSharper disable once ClassNeverInstantiated.Global
    public class SignalStart : Signal
    {
    }


// ReSharper disable once ClassNeverInstantiated.Global
    public class SignalDestroy : Signal // signal to destroy the current context
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
    public class SignalCriticalShutdown : Signal
    {
        
    }
}
