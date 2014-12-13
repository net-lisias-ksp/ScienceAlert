using System;
using UnityEngine;
namespace ScienceAlert.ProfileData.Implementations
{
    class ProfileManagerProvider : IProfileManagerProvider
    {
        public Maybe<IProfileManager> GetProfileManager()
        {
            ScienceAlertProfileManager pm = null;

            // treat profile manager as null until KSP has initialized it
            if (!ScenarioRunner.fetch.IsNull())
                pm = ScenarioRunner.fetch.GetComponent<ScienceAlertProfileManager>();
   
            return (!pm.IsNull() && pm.Ready) ? Maybe<IProfileManager>.With(pm) : Maybe<IProfileManager>.None;
        }
    }
}
