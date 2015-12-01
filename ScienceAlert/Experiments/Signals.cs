using ScienceAlert.Game;
using strange.extensions.signal.impl;

namespace ScienceAlert.Experiments
{
    public class SignalExperimentAvailable : Signal // todo: include info
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
