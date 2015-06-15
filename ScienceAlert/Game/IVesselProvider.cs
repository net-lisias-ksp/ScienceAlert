using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ReeperCommon.Containers;

namespace ScienceAlert.Game
{
    public interface IVesselProvider
    {
        Maybe<IVessel> Get();
    }
}
