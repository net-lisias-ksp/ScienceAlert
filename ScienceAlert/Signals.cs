using strange.extensions.signal.impl;

namespace ScienceAlert
{
// ReSharper disable once ClassNeverInstantiated.Global
    public class SignalStart : Signal
    {
    }


    public class SignalDestroy : Signal
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
}
