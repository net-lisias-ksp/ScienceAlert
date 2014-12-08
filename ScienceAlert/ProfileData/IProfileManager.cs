using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ScienceAlert.KSPInterfaces.FlightGlobals;

namespace ScienceAlert.ProfileData
{
    public interface IProfileManager
    {
        IProfile DefaultProfile { get; }
        IProfile ActiveProfile { get; }


        void OnSave(ConfigNode node);
        void OnLoad(ConfigNode node);

        void OnVesselCreate(IVessel vessel);
        void OnVesselDestroy(IVessel vessel);
        void OnVesselChange(IVessel vessel);
    }
}
