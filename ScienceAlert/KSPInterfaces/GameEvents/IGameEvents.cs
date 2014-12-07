using ScienceAlert.KSPInterfaces.FlightGlobals;

namespace ScienceAlert.KSPInterfaces.GameEvents
{
    public delegate void VesselEvent(IVessel vessel);

    public interface IGameEvents
    {
        event VesselEvent OnVesselChange;
        event VesselEvent OnVesselWasModified;
        event VesselEvent OnVesselDestroy;
    }
}
