using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ScienceAlert.KSPInterfaces.FlightGlobals.Implementations
{
    class KspActiveVesselProvider : IActiveVesselProvider
    {
        public Maybe<IVessel> GetActiveVessel()
        {
            return global::FlightGlobals.ActiveVessel.IsNull()
                ? Maybe<IVessel>.None
                : Maybe<IVessel>.With(new KspVessel(global::FlightGlobals.ActiveVessel));
        }
    }
}
