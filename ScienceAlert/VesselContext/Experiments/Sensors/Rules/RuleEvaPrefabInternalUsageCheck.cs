using System;
using System.Linq;
using JetBrains.Annotations;
using ReeperCommon.Containers;
using ScienceAlert.Game;
using UnityEngine;

namespace ScienceAlert.VesselContext.Experiments.Sensors.Rules
{
    /// <summary>
    /// It's technically possible for a ConfigNode patch or another mod to change the internal usage requirements for EVA reports
    /// (for example, only allowing scientists or whatever to do them).
    /// That's fine while on EVA because we can examine those requirements directly, but to handle them properly while not on EVA
    /// we'll have to look up the prefab. In 99.9% of cases nobody will mess with them though, this is just to catch the edge cases
    /// and it's not complex anyway
    /// 
    /// TODO: it's possible for male and female crew to differ in this setting so at some point the EVA trigger is going to need
    /// to be updated to choose correctly
    /// </summary>
    class RuleEvaPrefabInternalUsageCheck : ISensorRule
    {
        private const string EvaReportExperimentId = "evaReport";

        private class EvaPrefab
        {
            public readonly ExperimentUsageReqs Requirements;
            public readonly IPart Part;

            public EvaPrefab([NotNull] IPart part, ExperimentUsageReqs usageMask)
            {
                if (part == null) throw new ArgumentNullException("part");

                Requirements = usageMask;
                Part = part;
            }
        }

        private readonly IVessel _vessel;
        private readonly IScienceUtil _scienceUtil;

        [Flags]
        private enum CrewGenders
        {
            NoCrew = 0,
            Male = 1 << 0,
            Female = 1 << 1
        }

        private CrewGenders _crewGenders = CrewGenders.NoCrew;
        private readonly Lazy<EvaPrefab> _malePrefab = new Lazy<EvaPrefab>(() => GetEvaPrefab(ProtoCrewMember.Gender.Male));
        private readonly Lazy<EvaPrefab> _femalePrefab = new Lazy<EvaPrefab>(() => GetEvaPrefab(ProtoCrewMember.Gender.Female)); 


        public RuleEvaPrefabInternalUsageCheck(
            [NotNull] IVessel vessel, 
            [NotNull] IScienceUtil scienceUtil,
            SignalActiveVesselCrewModified crewModified)
        {
            if (vessel == null) throw new ArgumentNullException("vessel");
            if (scienceUtil == null) throw new ArgumentNullException("scienceUtil");

            _vessel = vessel;
            _scienceUtil = scienceUtil;
            crewModified.AddListener(OnCrewModified);
        }


        [PostConstruct]
        public void Setup()
        {
            OnCrewModified();
        }


        private static ExperimentUsageReqs GetUsageReqFromPrefab([NotNull] GameObject prefab)
        {
            if (prefab == null) throw new ArgumentNullException("prefab");

            return prefab
                .With(go => go.GetComponents<ModuleScienceExperiment>()
                    .FirstOrDefault(mse => mse.experimentID == EvaReportExperimentId))
                .Return(mse => (ExperimentUsageReqs)mse.usageReqMaskInternal, ExperimentUsageReqs.Never);
        }


        private static EvaPrefab GetEvaPrefab(ProtoCrewMember.Gender gender)
        {
            GameObject prefab = null;

            switch (gender)
            {
                case ProtoCrewMember.Gender.Male:
                    prefab = FlightEVA.fetch.evaPrefab_generic;
                    break;

                default:
                    prefab = FlightEVA.fetch.evaPrefab_female;
                    break;
            }

            return new EvaPrefab(new KspPart(Part.FromGO(prefab)), GetUsageReqFromPrefab(prefab));
        }


        private void OnCrewModified()
        {
            var crew = _vessel.EvaCapableCrew;

            if (!crew.Any())
            {
                _crewGenders = CrewGenders.NoCrew;
                return;
            }

            _crewGenders |= (crew.Any(pcm => pcm.gender == ProtoCrewMember.Gender.Male) ? CrewGenders.Male : 0);
            _crewGenders |= (crew.Any(pcm => pcm.gender == ProtoCrewMember.Gender.Female) ? CrewGenders.Female : 0);
        }


        public bool Passes()
        {
            if ((_crewGenders & CrewGenders.Male) != 0)
                return _scienceUtil.RequiredUsageInternalAvailable(_vessel, _malePrefab.Value.Part,
                    _malePrefab.Value.Requirements);
            return _scienceUtil.RequiredUsageInternalAvailable(_vessel, _femalePrefab.Value.Part,
                _femalePrefab.Value.Requirements);
        }
    }
}
