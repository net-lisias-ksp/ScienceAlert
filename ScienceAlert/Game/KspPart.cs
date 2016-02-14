using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace ScienceAlert.Game
{
    public class KspPart : IPart
    {
        private readonly Part _part;

        public KspPart(Part part, IVessel vessel)
        {
            if (part == null) throw new ArgumentNullException("Part");
            if (vessel == null) throw new ArgumentNullException("vessel");

            _part = part;
            Vessel = vessel;
        }

        public List<ProtoCrewMember> EvaCapableCrew
        {
            get { return _part.protoModuleCrew; }
        }

        public IVessel Vessel { get; private set; }
    }
}
