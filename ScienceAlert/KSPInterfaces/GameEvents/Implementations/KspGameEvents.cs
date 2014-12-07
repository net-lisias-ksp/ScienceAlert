using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ScienceAlert.KSPInterfaces.FlightGlobals.Implementations;

namespace ScienceAlert.KSPInterfaces.GameEvents.Implementations
{
    class KspGameEvents : IGameEvents
    {
        public event VesselEvent OnVesselChange;
        public event VesselEvent OnVesselWasModified;
        public event VesselEvent OnVesselDestroy;

        public KspGameEvents()
        {
            global::GameEvents.onVesselChange.Add(OnVesselChangeAdapter);
            global::GameEvents.onVesselWasModified.Add(OnVesselWasModifiedAdapter);
            global::GameEvents.onVesselDestroy.Add(OnVesselDestroyAdapter);
        }

        ~KspGameEvents()
        {
            global::GameEvents.onVesselChange.Remove(OnVesselChangeAdapter);
            global::GameEvents.onVesselWasModified.Remove(OnVesselWasModifiedAdapter);
            global::GameEvents.onVesselDestroy.Remove(OnVesselDestroyAdapter);
        }

        private void OnVesselChangeAdapter(Vessel v)
        {
            OnVesselChange(new KspVessel(v));
        }

        private void OnVesselWasModifiedAdapter(Vessel v)
        {
            OnVesselWasModified(new KspVessel(v));
        }

        private void OnVesselDestroyAdapter(Vessel v)
        {
            OnVesselDestroy(new KspVessel(v));
        }
    }
}
