using strange.extensions.signal.impl;

namespace ScienceAlert
{
// ReSharper disable once ClassNeverInstantiated.Global
    public class SignalStart : Signal
    {
    }


    public class SignalDestroy : Signal // signal to destroy the current context
    {

    }


    public class SignalVesselChanged : Signal<IVessel>
    {
    }

    public class SignalVesselModified : Signal<IVessel>
    {

    }

    public class SignalVesselDestroyed : Signal<IVessel>
    {

    }


    public class SignalShutdownScienceAlert : Signal
    {
        
    }


    public class SignalGameTick : Signal // regular MonoBehaviour.Update
    {

    }


    
}
