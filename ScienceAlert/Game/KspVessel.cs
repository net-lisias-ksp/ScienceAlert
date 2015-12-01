using System;
using ScienceAlert.Experiments;

namespace ScienceAlert.Game
{
    public class KspVessel : IVessel
    {
        private readonly Vessel _vessel;

        public KspVessel(Vessel vessel)
        {
            if (vessel == null) throw new ArgumentNullException("vessel");

            _vessel = vessel;
        }


        public override int GetHashCode()
        {
            return _vessel.GetHashCode();
        }


        public override bool Equals(object obj)
        {
            if (!(obj is KspVessel)) return false;

            var other = obj as KspVessel;

            return ReferenceEquals(_vessel, other._vessel);
        }
    }
}
