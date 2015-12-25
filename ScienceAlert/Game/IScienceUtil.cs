using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ScienceAlert.Game
{
    public interface IScienceUtil
    {
        bool RequiredUsageInternalAvailable(IVessel vessel, IPart part, ExperimentUsageReqs reqs);
    }
}
