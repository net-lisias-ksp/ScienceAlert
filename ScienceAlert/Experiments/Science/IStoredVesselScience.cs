using System.Collections.Generic;
using ScienceAlert.KSPInterfaces.FlightGlobals;

namespace ScienceAlert.Experiments.Science
{
    public interface IStoredVesselScience
    {
        void OnVesselChange(IVessel vessel);
        void OnVesselWasModified(IVessel vessel);
        void OnVesselDestroy(IVessel vessel);

        IEnumerable<ScienceData> ScienceData { get; }
    }
}