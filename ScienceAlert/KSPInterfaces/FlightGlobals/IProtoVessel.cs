using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ScienceAlert.KSPInterfaces.FlightGlobals
{
    public interface IProtoVessel
    {
        int rootIndex { get; }

        List<ProtoPartSnapshot> protoPartSnapshots { get; }
    }
}
