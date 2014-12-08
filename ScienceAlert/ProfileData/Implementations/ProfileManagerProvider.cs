using System;
using UnityEngine;
namespace ScienceAlert.ProfileData.Implementations
{
    class ProfileManagerProvider : IProfileManagerProvider
    {
        public Maybe<IProfileManager> GetProfileManager()
        {
            return ScienceAlertProfileManager.Instance.IsNull()
                ? Maybe<IProfileManager>.None
                : Maybe<IProfileManager>.With(ScienceAlertProfileManager.Instance);
        }
    }
}
