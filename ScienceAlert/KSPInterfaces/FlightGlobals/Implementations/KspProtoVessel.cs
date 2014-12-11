using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ScienceAlert.KSPInterfaces.FlightGlobals.Implementations
{
    class KspProtoVessel : IProtoVessel
    {
        private readonly ProtoVessel _proto;

        public KspProtoVessel(ProtoVessel pv)
        {
            if (pv.IsNull())
                throw new ArgumentNullException("pv");

            _proto = pv;    
        }

        public int rootIndex { get { return _proto.rootIndex; } }
        public List<ProtoPartSnapshot> protoPartSnapshots { get { return _proto.protoPartSnapshots; } }
    }
}
