using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ScienceAlert.Game
{
    public class KspPart : IPart
    {
        private readonly Part _part;

        public KspPart(Part part)
        {
            if (part == null) throw new ArgumentNullException("part");

            _part = part;
        }

        public GameObject gameObject
        {
            get { return _part.gameObject; }
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
