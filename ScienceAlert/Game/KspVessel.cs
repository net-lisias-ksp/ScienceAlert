using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ScienceAlert.Annotations;

namespace ScienceAlert.Game
{
    public class KspVessel : IVessel
    {
        private readonly Vessel _vessel;

        public KspVessel([NotNull] Vessel vessel)
        {
            if (vessel == null) throw new ArgumentNullException("vessel");
            _vessel = vessel;
        }


        public IEnumerable<T> FindPartModulesImplementing<T>() where T : class
        {
            return _vessel.FindPartModulesImplementing<T>();
        }
    }
}
