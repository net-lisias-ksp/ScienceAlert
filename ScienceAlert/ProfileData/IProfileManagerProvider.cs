using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ScienceAlert.ProfileData
{
    public interface IProfileManagerProvider
    {
        Maybe<IProfileManager> GetProfileManager();
    }
}
