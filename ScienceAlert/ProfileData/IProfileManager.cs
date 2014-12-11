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
        int Count { get; }
        Dictionary<string, IProfile> Profiles { get; }

        bool HaveStoredProfile(string name);
        void StoreActiveProfile(string name);
        void DeleteProfile(string name);
        void RenameProfile(string from, string to);
        bool AssignAsActiveProfile(IProfile profile);

        void OnSave(ConfigNode node);
        void OnLoad(ConfigNode node);

        void OnVesselCreate(IVessel vessel);
        void OnVesselDestroy(IVessel vessel);
        void OnVesselChange(IVessel vessel);
    }
}
