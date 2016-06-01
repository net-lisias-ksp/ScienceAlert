using System;
using System.Linq;
using JetBrains.Annotations;
using ReeperCommon.Containers;
using ReeperCommon.Logging;
using ScienceAlert.Game;

namespace ScienceAlert.VesselContext.Experiments.Rules
{
    /// <summary>
    /// It's technically possible for a ConfigNode patch or another mod to change the internal usage requirements for EVA reports
    /// (for example, only allowing scientists or whatever to do them).
    /// That's fine while on EVA because we can examine those requirements directly, but to handle them properly while not on EVA
    /// we'll have to look up the prefab. In 99.9% of cases nobody will mess with them though, this is just to catch the edge cases
    /// and it's not complex anyway
    /// </summary>
    class RuleEvaPrefabInternalUsageCheck : ISensorRule
    {
        private const string EvaReportExperimentId = "evaReport";

        private readonly IVessel _vessel;
        private readonly IScienceUtil _scienceUtil;
        private readonly IPartLoader _partLoader;
        private readonly ICriticalShutdownEvent _missingEva;
        private readonly Lazy<IPart> _evaPrefab;
        private readonly Lazy<ExperimentUsageReqs> _requirements;

        public RuleEvaPrefabInternalUsageCheck(
            [NotNull] IVessel vessel, 
            [NotNull] IScienceUtil scienceUtil,
            [NotNull] IPartLoader partLoader, 
            [NotNull] ICriticalShutdownEvent missingEva)
        {
            if (vessel == null) throw new ArgumentNullException("vessel");
            if (scienceUtil == null) throw new ArgumentNullException("scienceUtil");
            if (partLoader == null) throw new ArgumentNullException("partLoader");
            if (missingEva == null) throw new ArgumentNullException("missingEva");

            _vessel = vessel;
            _scienceUtil = scienceUtil;
            _partLoader = partLoader;
            _missingEva = missingEva;
            _evaPrefab = new Lazy<IPart>(() => GetPrefab("kerbalEVA"));
            _requirements = new Lazy<ExperimentUsageReqs>(GetEvaUsageReqs);
        }


        private IPart GetPrefab(string name)
        {
            try
            {
                var prefab = _partLoader.GetPartByName(name);

                if (!prefab.Any())
                    throw new ArgumentException("No part prefab matches '" + name + "'");

                return prefab.Value;
            }
            catch (Exception)
            {
                Log.Error("Could not find EVA prefab!");
                _missingEva.Dispatch();

                throw;
            }
        }


        private ExperimentUsageReqs GetEvaUsageReqs()
        {
            try
            {
                return (ExperimentUsageReqs)
                    _evaPrefab.Value.gameObject.GetComponents<ModuleScienceExperiment>() // can't use .Modules here, might not be initialized on the prefab
                        .First(mse => mse.experimentID == EvaReportExperimentId)
                        .usageReqMaskInternal;
            }
            catch (Exception) // no EVA report experiment found. This might be fine if another mod has intentionally removed it
            {
                return ExperimentUsageReqs.Never;
            }
        }


        public bool Passes()
        {
            return _scienceUtil.RequiredUsageInternalAvailable(_vessel, _evaPrefab.Value, _requirements.Value);
        }
    }
}
