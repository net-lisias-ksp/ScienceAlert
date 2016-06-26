using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
            IsCommandSeat = _part.Modules.Contains<KerbalSeat>();
        }

        public GameObject gameObject
        {
            get { return _part.gameObject; }
        }

        public ReadOnlyCollection<ProtoCrewMember> EvaCapableCrew
        {
            get
            {
                return _part.protoModuleCrew
                    .Where(pcm => pcm.type == ProtoCrewMember.KerbalType.Crew)
                    .ToList()
                    .AsReadOnly();
            }
        }


        public ReadOnlyCollection<PartModule> Modules
        {
            get { return new List<PartModule>(_part.Modules.Cast<PartModule>()).ToList().AsReadOnly(); }
        }

        public bool IsCommandSeat { get; private set; }

        public bool IsShieldedFromAirstream
        {
            get { return _part.ShieldedFromAirstream; }
        }
    }
}
