using System;
using System.Collections.Generic;

namespace ScienceAlert.Game
{
    public class KspPart : IPart
    {
        private readonly Part _part;

        public KspPart(Part part)
        {
            if (part == null) throw new ArgumentNullException("Part");

            _part = part;
        }

        public List<ProtoCrewMember> EvaCapableCrew
        {
            get { return _part.protoModuleCrew; }
        }

    }
}
