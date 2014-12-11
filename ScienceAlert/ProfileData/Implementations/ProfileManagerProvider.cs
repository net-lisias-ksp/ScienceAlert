using System;
using UnityEngine;
namespace ScienceAlert.ProfileData.Implementations
{
    class ProfileManagerProvider : IProfileManagerProvider
    {
        public Maybe<IProfileManager> GetProfileManager()
        {
            // treat profile manager as null until KSP has initialized it
            var pm = ScenarioRunner.fetch.GetComponent<ScienceAlertProfileManager>();
   
            return pm.Ready ? Maybe<IProfileManager>.With(pm) : Maybe<IProfileManager>.None;
        }
    }
}
