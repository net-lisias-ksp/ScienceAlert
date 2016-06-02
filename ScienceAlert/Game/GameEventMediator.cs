using System;
using ReeperCommon.Containers;
using ReeperCommon.Logging;
using strange.extensions.mediation.impl;

namespace ScienceAlert.Game
{
    public class GameEventMediator : Mediator
    {
// ReSharper disable MemberCanBePrivate.Global

        [Inject] public GameEventView View { get; set; }
        [Inject] public IGameFactory GameFactory { get; set; }

        [Inject] public SignalVesselChanged VesselChangedSignal { get; set; }
        [Inject] public SignalVesselModified VesselModifiedSignal { get; set; }

        [Inject] public SignalActiveVesselModified ActiveVesselModifiedSignal { get; set; }
        [Inject] public SignalActiveVesselDestroyed ActiveVesselDestroyedSignal { get; set; }

        [Inject] public SignalActiveVesselCrewModified ActiveVesselCrewModifiedSignal { get; set; }
        [Inject] public SignalCrewBoardVessel CrewBoardVesselSignal { get; set; }
        [Inject] public SignalCrewOnEva CrewOnEvaSignal { get; set; }
        [Inject] public SignalCrewTransferred CrewTransferredSignal { get; set; }
        [Inject] public SignalCrewKilled CrewKilledSignal { get; set; }

        [Inject] public SignalDominantBodyChanged DominantBodyChangedSignal { get; set; }

        [Inject] public SignalGameSceneLoadRequested GameSceneLoadRequestedSignal { get; set; }
        [Inject] public SignalScienceReceived ScienceReceivedSignal { get; set; }

        [Inject] public SignalApplicationQuit ApplicationQuitSignal { get; set; }
        [Inject] public SignalGameTick GameTickSignal { get; set; }

        public override void OnRegister()
        {
            Log.Warning("GameEventMediator.OnRegister");

            base.OnRegister();
            View.VesselModified.AddListener(OnVesselModified);
            View.ActiveVesselChanged.AddListener(OnActiveVesselChanged);
            View.ActiveVesselModified.AddListener(OnActiveVesselModified);
            View.ActiveVesselDestroyed.AddListener(OnActiveVesselDestroyed);
            View.GameSceneLoadRequested.AddListener(OnGameSceneLoadRequested);
            View.ScienceReceived.AddListener(OnScienceReceived);
            View.ActiveVesselCrewModified.AddListener(OnActiveVesselCrewModified);
            View.CrewOnEva.AddListener(OnCrewOnEva);
            View.CrewTransferred.AddListener(OnCrewTransferred);
            View.CrewKilled.AddListener(OnCrewKilled);
            View.CrewBoardVessel.AddListener(OnCrewBoardVessel);
            View.DominantBodyChanged.AddListener(OnDominantBodyChanged);

            View.ApplicationQuit.AddListener(OnApplicationQuit);
            View.GameUpdateTick.AddListener(OnGameUpdateTick);
        }




        public override void OnRemove()
        {
            View.VesselModified.RemoveListener(OnVesselModified);
            View.ActiveVesselChanged.RemoveListener(OnActiveVesselChanged);
            View.ActiveVesselModified.RemoveListener(OnActiveVesselModified);
            View.ActiveVesselDestroyed.RemoveListener(OnActiveVesselDestroyed);
            View.GameSceneLoadRequested.RemoveListener(OnGameSceneLoadRequested);
            View.ScienceReceived.RemoveListener(OnScienceReceived);
            View.ActiveVesselCrewModified.RemoveListener(OnActiveVesselCrewModified);
            View.CrewOnEva.RemoveListener(OnCrewOnEva);
            View.CrewTransferred.RemoveListener(OnCrewTransferred);
            View.CrewKilled.RemoveListener(OnCrewKilled);
            View.CrewBoardVessel.RemoveListener(OnCrewBoardVessel);
            View.DominantBodyChanged.RemoveListener(OnDominantBodyChanged);

            View.ApplicationQuit.RemoveListener(OnApplicationQuit);
            View.GameUpdateTick.RemoveListener(OnGameUpdateTick);
            base.OnRemove();
        }


        /// <summary>
        /// Because throwing an exception inside a GameEvent could have serious gamebreaking 
        /// consequences, we'll make sure anything that does get thrown by SA is trapped before
        /// it can unwind far enough to throw a wrench into KSP code
        /// </summary>
        /// <param name="action"></param>
        private void TryAction(Action action)
        {
            try
            {
                action();
            }
            catch (Exception e)
            {
                Log.Error("Encountered exception: " + e);
            }
        }


        private void OnVesselModified(Vessel vessel)
        {
            TryAction(() => VesselModifiedSignal.Dispatch(GameFactory.Create(vessel)));
        }

        private void OnActiveVesselChanged()
        {
            Log.Debug("GameEventMediator.OnActiveVesselChanged");

            TryAction(() => VesselChangedSignal.Dispatch());
        }



        private void OnActiveVesselModified()
        {
            Log.Debug("GameEventMediator.OnActiveVesselModified - dispatching active vessel modified signal");
            TryAction(() => ActiveVesselModifiedSignal.Dispatch());
        }


        private void OnActiveVesselDestroyed()
        {
            Log.Debug("GameEventMediator.OnActiveVesselDestroyed - dispatching active vessel destroyed signal");
            TryAction(() => ActiveVesselDestroyedSignal.Dispatch());
        }



        private void OnCrewOnEva(GameEvents.FromToAction<Part, Part> fromToAction)
        {
            Log.Debug("GameEventMediator.OnCrewOnEva");

            TryAction(() => CrewOnEvaSignal.Dispatch(
                new GameEvents.FromToAction<IPart, IPart>(
                    fromToAction.from.With(p => GameFactory.Create(p)),
                    fromToAction.to.With(p => GameFactory.Create(p)))));
        }


        private void OnCrewTransferred(GameEvents.HostedFromToAction<ProtoCrewMember, Part> hostedFromToAction)
        {
            Log.Debug("GameEventMediator.OnCrewTransferred");

            TryAction(() => CrewTransferredSignal.Dispatch(
                new GameEvents.HostedFromToAction<ProtoCrewMember, IPart>(
                    hostedFromToAction.host,
                    hostedFromToAction.from.With(p => GameFactory.Create(p)),
                    hostedFromToAction.to.With(p => GameFactory.Create(p)))));
        }


        private void OnGameSceneLoadRequested(GameScenes data)
        {
            TryAction(() => GameSceneLoadRequestedSignal.Dispatch(data));
        }


        private void OnScienceReceived(float data0, ScienceSubject data1, ProtoVessel data2, bool data3)
        {
            TryAction(() => ScienceReceivedSignal.Dispatch(data0, new KspScienceSubject(data1), data2, data3));
        }


        private void OnCrewBoardVessel(GameEvents.FromToAction<Part, Part> fromToAction)
        {
            CrewBoardVesselSignal.Dispatch(new GameEvents.FromToAction<IPart, IPart>(GameFactory.Create(fromToAction.from), GameFactory.Create(fromToAction.to)));
        }


        private void OnCrewKilled(EventReport eventReport)
        {
            CrewKilledSignal.Dispatch(eventReport);
        }


        private void OnActiveVesselCrewModified()
        {
            ActiveVesselCrewModifiedSignal.Dispatch();
        }


        private void OnDominantBodyChanged(GameEvents.FromToAction<CelestialBody, CelestialBody> data)
        {
            DominantBodyChangedSignal.Dispatch(
                new GameEvents.FromToAction<ICelestialBody, ICelestialBody>(GameFactory.Create(data.from),
                    GameFactory.Create(data.to)));
        }


        private void OnApplicationQuit()
        {
            Log.Debug(() => typeof (GameEventMediator).Name + ".OnApplicationQuit");
            ApplicationQuitSignal.Dispatch(); // wrapper not necessary because this isn't a GameEvent supplied by the game
        }


        private void OnGameUpdateTick()
        {
            GameTickSignal.Dispatch(); // wrapper not necessary because this isn't a GameEvent supplied by the game
        }
    }
}
