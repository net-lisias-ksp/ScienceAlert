using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using ReeperCommon.Containers;
using ReeperCommon.Logging;
using strange.extensions.promise.api;
using strange.extensions.promise.impl;
using ScienceAlert.Game;
using UnityEngine;

namespace ScienceAlert.VesselContext.Experiments.Triggers
{
    // ReSharper disable once UnusedMember.Global
    class EvaDeploymentTrigger : DefaultExperimentTrigger
    {
        // A little helper to figure out the best candidate for EVA. We'll try and use the best scientist we can
        // for reasons
        private class CandidateComparer : Comparer<ProtoCrewMember>
        {
            private enum TraitValues : int
            {
                Unknown = 0,
                Pilot = 1 << 1,     // L5 pilot = 2 + 5
                Engineer = 1 << 3, // L5 engineer = 8 + 5
                Scientist = 1 << 4 // L5 scientist = 16 + 5
            }

            private readonly Dictionary<string, int> _scores;

            public CandidateComparer()
            {
                _scores =
                    Enum.GetValues(typeof(TraitValues))
                        .Cast<TraitValues>()
                        .ToDictionary(tv => Enum.GetName(typeof(TraitValues), tv), tv => (int)tv);
            }

            public override int Compare(ProtoCrewMember first, ProtoCrewMember second)
            {
                var firstScore = GetScore(first);
                var secondScore = GetScore(second);

                if (firstScore == secondScore) return 0;

                if (firstScore < secondScore)
                    return -1;
                return 1;
            }


            private int GetScore(ProtoCrewMember pcm)
            {
                int score = 0;

                _scores.TryGetValue(pcm.trait, out score);

                return score;
            }
        }

        private struct EvaCandidate
        {
            public readonly ProtoCrewMember Crew;
            public readonly Part From;
            public readonly Transform Airlock;

            public EvaCandidate([NotNull] ProtoCrewMember pcm, [NotNull] Part fromPart,
                [NotNull] Transform airlockTransform)
                : this()
            {
                if (pcm == null) throw new ArgumentNullException("pcm");
                if (fromPart == null) throw new ArgumentNullException("fromPart");
                if (airlockTransform == null) throw new ArgumentNullException("airlockTransform");
                if (pcm.type != ProtoCrewMember.KerbalType.Crew)
                    throw new ArgumentException(
                        "Crew member cannot be an applicant, tourist or unowned. How did this even happen?", "pcm");

                Crew = pcm;
                From = fromPart;
                Airlock = airlockTransform;
            }
        }


        public EvaDeploymentTrigger([NotNull] ScienceExperiment experiment, [NotNull] IVessel activeVessel, [NotNull] IScienceUtil scienceUtil) : base(experiment, activeVessel, scienceUtil)
        {
        }


        public override IPromise Deploy()
        {
            if (IsBusy)
                throw new TriggerIsBusyException(Experiment);

            if (ActiveVessel.isEVA)
            {
                Log.Warning(GetType().Name + ".Deploy: active vessel is already EVA, using default");
                return base.Deploy();
            }
            Log.Warning(GetType().Name + ".Deploy: active vessel is not EVA, deploying EVA");

            var promise = new Promise();
            UnfulfilledPromise = Maybe<IPromise>.None;

            // dump a Kerbal out an airlock
            // this turns out to be slightly more involved than you'd think
            try
            {
                var candidate = GetSuitableEvaCandidate();
                UnfulfilledPromise = Maybe<IPromise>.With(promise);

                if (candidate.Any())
                    SendCrewmanOnEva(candidate.Value, promise);
                else promise.ReportFail(new NoSuitableEvaCandidateFoundException());
            }
            catch (Exception e)
            {
                promise.ReportFail(e);
            }
            return promise;
        }


        private static void SendCrewmanOnEva(EvaCandidate lucky, IPromise promise)
        {
            EnsureNotInMapView();
            EnsureNotInIvaView();

            if (!FlightEVA.fetch.spawnEVA(lucky.Crew, lucky.From, lucky.Airlock))
                promise.ReportFail(new InvalidOperationException("Could not send " + lucky.Crew.name + " on EVA"));
            else
            {
                // todo: delete kerbal in part if it was duplicated somehow? normally happens if another mod throws
                // an exception while EVAing

                promise.Dispatch();
            }
        }


        // Sort by the most preferred candidate (high level scientists, then engineers, then pilots)
        // that are in a part containing an airlock
        private Maybe<EvaCandidate> GetSuitableEvaCandidate()
        {
            var candidatesBestFirst = ActiveVessel.EvaCapableCrew
                .OrderBy(pcm => pcm, new CandidateComparer());

            foreach (var candidate in candidatesBestFirst)
            {
                var part = GetOccupiedPart(candidate);

                if (!part.Any()) continue;
                if (part.Value.airlock == null) continue;

                return new EvaCandidate(candidate, part.Value, part.Value.airlock).ToMaybe();
            }

            return Maybe<EvaCandidate>.None;
        }


        private Maybe<Part> GetOccupiedPart(ProtoCrewMember pcm)
        {
            return
                ActiveVessel.Parts.FirstOrDefault(
                    part =>
                        part.protoModuleCrew.Contains(pcm) ||
                        part.protoModuleCrew.Any(protoCrew => protoCrew.name == pcm.name)).ToMaybe();
        }


        private static void EnsureNotInMapView()
        {
            if (MapView.MapIsEnabled)
                MapView.ExitMapView();
        }


        private static void EnsureNotInIvaView()
        {
            var currentMode = CameraManager.Instance.currentCameraMode;

            if (currentMode == CameraManager.CameraMode.Internal || currentMode == CameraManager.CameraMode.IVA)
                CameraManager.Instance.SetCameraFlight();
        }
    }
}
