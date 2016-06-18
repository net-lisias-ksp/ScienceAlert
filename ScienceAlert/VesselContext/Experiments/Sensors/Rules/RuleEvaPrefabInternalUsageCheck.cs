using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Experience;
using JetBrains.Annotations;
using ReeperCommon.Containers;
using ScienceAlert.Game;
using UnityEngine;

namespace ScienceAlert.VesselContext.Experiments.Sensors.Rules
{
    /// <summary>
    /// Special version of the experiment usage check that examines the EVA prefabs for internal usage reqs
    /// if the Vessel itself isn't an EVA. 
    /// 
    /// Why this is needed is best explained with an example. Consider we have some experiment performed while on EVA
    /// that has a certain restriction. Let's say that taking surface samples is now restricted to scientists only. The
    /// sensors for this experiment have been configured such that it's considered to be available even when not controlling
    /// an EVA'ing scientist. If the player is inside a ship (which does NOT have the ModuleScienceExperiment for surface samples),
    /// how can we tell if the usage requirements have been met? They're defined by a specific MSE which the ship doesn't have. 
    /// 
    /// We'll have to look at the EVA prefabs. But then, we also don't know which crewmember will go out on EVA. What if the vessel
    /// has crew, but none of them are scientists? That only matters if we need one, but if we do need one we'll have to create a 
    /// fake one because the EVA part doesn't actually exist, and then you can see how quickly a specific rule to handle such
    /// an edge case becomes necessary
    /// </summary>
    class RuleEvaPrefabInternalUsageCheck : RuleExperimentUsageRequirementCheck
    {
        // ExperienceTrait uses .TypeName or its Config.Title to identify the trait. I don't want to create my own
        // Scientist : ExperienceTrait because that might break something if somebody tries to identify possible ExerienceTraits
        // using reflection while SA is installed.
        //
        // Instead, I'll create a fake one that mimics the real thing by abusing the way ExperienceTrait will use the the title
        // of its Config rather than the Type.Name if its Type is ExperienceTrait (plain underived version)
        private struct FakeExperienceTraitConfig
        {
            [Persistent (name = "name")] public string _name;
            [Persistent (name = "title")] public string _title;

            public static ExperienceTraitConfig Create(string desiredTitle)
            {
                return
                    ExperienceTraitConfig.Create(
                        ConfigNode.CreateConfigFromObject(new FakeExperienceTraitConfig
                        {
                            _name = "UnusedButRequiredOrExceptionGetsThrown",
                            _title = desiredTitle
                        }));
            }
        }

        private class PrefabScienceModule : IModuleScienceExperiment
        {
            public PrefabScienceModule(ExperimentUsageReqs internalUsageMask)
            {
                InternalUsageRequirements = internalUsageMask;
            }


            public ExperimentUsageReqs InternalUsageRequirements { get; private set; }

            #region parts we don't need

            public IPart Part
            {
                get { throw new NotImplementedException(); }
            }

            public string ModuleTypeName
            {
                get { throw new NotImplementedException(); }
            }

            public bool Deployed
            {
                get { throw new NotImplementedException(); }
            }

            public string ExperimentID
            {
                get { throw new NotImplementedException(); }
            }


            public bool CanBeDeployed
            {
                get { throw new NotImplementedException(); }
            }

            public float TransmissionMultiplier
            {
                get { throw new NotImplementedException(); }
            }

            public Maybe<int[]> FxIndices
            {
                get { throw new NotImplementedException(); }
            }

            public void Deploy()
            {
                throw new NotImplementedException();
            }
            #endregion
        }

        private class EvaPrefabPart : IPart
        {
            private readonly ReadOnlyCollection<IModuleScienceExperiment> _experimentModules;

            private readonly ReadOnlyCollection<ProtoCrewMember> _crewList = new List<ProtoCrewMember>
            {
                new ProtoCrewMember(ProtoCrewMember.KerbalType.Crew)
            }.AsReadOnly();

            private bool _containsScientist = false;

            public GameObject gameObject
            {
                get { return null; }
            }

            public ReadOnlyCollection<ProtoCrewMember> EvaCapableCrew
            {
                get { return _crewList; }
            }

            public ReadOnlyCollection<PartModule> Modules
            {
                get { throw new System.NotImplementedException(); }
            }

            public EvaPrefabPart([NotNull] ReadOnlyCollection<IModuleScienceExperiment> experimentModules)
            {
                if (experimentModules == null) throw new ArgumentNullException("experimentModules");
                _experimentModules = experimentModules;
            }


            private void SetupFakeCrewMemberTrait(bool isScientist)
            {
                var cfg =
                    FakeExperienceTraitConfig.Create(isScientist ? CrewTraitNames.ScientistTypeName : "NotAScientist");

                _crewList.First().experienceTrait = ExperienceTrait.Create(typeof (ExperienceTrait), cfg, _crewList.First());
                _containsScientist = isScientist;
            }


            public bool ContainsUsableExperimentModule([NotNull] IVessel activeVessel, [NotNull] IScienceUtil util)
            {
                if (activeVessel == null) throw new ArgumentNullException("activeVessel");
                if (util == null) throw new ArgumentNullException("util");

                if (activeVessel.HasScientist != _containsScientist)
                    SetupFakeCrewMemberTrait(activeVessel.HasScientist);

                // ReSharper disable once LoopCanBeConvertedToQuery
                foreach (var exp in _experimentModules)
                    if (util.RequiredUsageInternalAvailable(activeVessel, this, exp.InternalUsageRequirements))
                        return true;
                return false;
            }
        }


        private readonly Lazy<EvaPrefabPart> _maleEvaPrefab;
        private readonly Lazy<EvaPrefabPart> _femaleEvaPrefab;
 
        public RuleEvaPrefabInternalUsageCheck(IScienceUtil scienceUtil, ScienceExperiment experiment, IVessel vessel) : base(scienceUtil, experiment, vessel)
        {
            _maleEvaPrefab = new Lazy<EvaPrefabPart>(() => CreateEvaPrefabPseudoPart(FlightEVA.fetch.evaPrefab_generic));
            _femaleEvaPrefab = new Lazy<EvaPrefabPart>(() => CreateEvaPrefabPseudoPart(FlightEVA.fetch.evaPrefab_female));
        }

        private EvaPrefabPart CreateEvaPrefabPseudoPart(GameObject prefab)
        {
            return new EvaPrefabPart(prefab.GetComponents<ModuleScienceExperiment>()
                        .Where(mse => mse.experimentID.Equals(Experiment.id, StringComparison.Ordinal))
                        .Select(mse => (IModuleScienceExperiment)new PrefabScienceModule((ExperimentUsageReqs)mse.usageReqMaskInternal))
                        .ToList()
                        .AsReadOnly());
        }



        public override bool Passes()
        {
            if (Vessel.isEVA)
                return base.Passes();

            if (Vessel.EvaCapableCrew.Any(pcm => pcm.gender == ProtoCrewMember.Gender.Male))
                if (_maleEvaPrefab.Value.ContainsUsableExperimentModule(Vessel, ScienceUtil))
                    return true;

            if (Vessel.EvaCapableCrew.Any(pcm => pcm.gender == ProtoCrewMember.Gender.Female))
                if (_femaleEvaPrefab.Value.ContainsUsableExperimentModule(Vessel, ScienceUtil))
                    return true;

            return false;
        }
    }
}
