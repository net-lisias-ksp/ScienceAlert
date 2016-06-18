using System;
using System.Linq;
using JetBrains.Annotations;
using ReeperCommon.Containers;
using ScienceAlert.Game;

namespace ScienceAlert.VesselContext.Experiments.Sensors.Rules
{
    /// <summary>
    /// Makes sure EVA is possible: we need both EVA capable crew and either be landed at Kerbin or have EVA unlocked eva via AC
    /// </summary>
    public class RuleEvaPossibilityCheck : ISensorRule
    {
        private readonly IVessel _activeVessel;
        private readonly Lazy<bool> _evaUnlocked = new Lazy<bool>(EvaIsUnlocked); 
      
        public RuleEvaPossibilityCheck([NotNull] IVessel activeVessel)
        {
            if (activeVessel == null) throw new ArgumentNullException("activeVessel");
            _activeVessel = activeVessel;
        }


        private static bool EvaIsUnlocked()
        {
            return GameVariables.Instance.UnlockedEVA(ScenarioUpgradeableFacilities.GetFacilityLevel(SpaceCenterFacility.AstronautComplex));
        }

        public bool Passes()
        {
            return GameVariables.Instance.EVAIsPossible(_evaUnlocked.Value, FlightGlobals.ActiveVessel) &&
                   _activeVessel.EvaCapableCrew.Any();
        }
    }
}
