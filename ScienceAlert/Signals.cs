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


    public class SignalActiveVesselModified : Signal
    {
    }




// ReSharper disable once ClassNeverInstantiated.Global
    public class SignalVesselDestroyed : Signal<IVessel>
    {
    }


    public class SignalActiveVesselDestroyed : Signal
    {
    }


// ReSharper disable once ClassNeverInstantiated.Global
    public class SignalShutdownScienceAlert : Signal
    {
        
    }
}
