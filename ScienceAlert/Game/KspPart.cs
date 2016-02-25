using System;
using System.Collections.Generic;
using System.Linq;

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


        public List<PartModule> Modules
        {
            get { return new List<PartModule>(_part.Modules.Cast<PartModule>()); }
        }

    }
}
