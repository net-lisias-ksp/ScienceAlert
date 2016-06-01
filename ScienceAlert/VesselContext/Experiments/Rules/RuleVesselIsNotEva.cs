using System;
using JetBrains.Annotations;
using ScienceAlert.Game;

namespace ScienceAlert.VesselContext.Experiments.Rules
{
    // ReSharper disable once UnusedMember.Global
    class RuleVesselIsNotEva : ISensorRule
    {
        private readonly IVessel _activeVessel;

        public RuleVesselIsNotEva(IVessel activeVessel)
        {
            if (activeVessel == null) throw new ArgumentNullException("activeVessel");
            _activeVessel = activeVessel;
        }

        public bool Passes()
        {
            return _activeVessel != null && !_activeVessel.isEVA;
        }
    }
}
