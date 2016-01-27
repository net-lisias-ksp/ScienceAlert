using System;
using ScienceAlert.Core;
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
        [Inject] public SignalVesselDestroyed VesselDestroyedSignal { get; set; }
        [Inject] public SignalActiveVesselDestroyed ActiveVesselDestroyedSignal { get; set; }
        [Inject] public SignalGameSceneLoadRequested GameSceneLoadRequestedSignal { get; set; }
        [Inject] public SignalScienceReceived ScienceReceivedSignal { get; set; }

        [Inject] public SignalApplicationQuit ApplicationQuitSignal { get; set; }
        [Inject] public SignalGameTick GameTickSignal { get; set; }

        public override void OnRegister()
        {
            base.OnRegister();
            View.VesselChanged.AddListener(OnVesselChanged);
            View.VesselModified.AddListener(OnVesselModified);
            View.VesselDestroyed.AddListener(OnVesselDestroyed);
            View.GameSceneLoadRequested.AddListener(OnGameSceneLoadRequested);
            View.ScienceReceived.AddListener(OnScienceReceived);

            View.ApplicationQuit.AddListener(OnApplicationQuit);
            View.GameUpdateTick.AddListener(OnGameUpdateTick);
        }


        public override void OnRemove()
        {
            View.VesselChanged.RemoveListener(OnVesselChanged);
            View.VesselModified.RemoveListener(OnVesselModified);
            View.VesselDestroyed.RemoveListener(OnVesselDestroyed);
            View.GameSceneLoadRequested.RemoveListener(OnGameSceneLoadRequested);
            View.ScienceReceived.RemoveListener(OnScienceReceived);

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


        private void OnVesselChanged(Vessel data)
        {
            Log.Debug("GameEventMediator.OnVesselChanged");

            TryAction(() => VesselChangedSignal.Dispatch(GameFactory.Create(data)));
        }



        private void OnVesselModified(Vessel data)
        {
            Log.Debug("GameEventMediator.OnVesselModified");

            TryAction(() => VesselModifiedSignal.Dispatch(GameFactory.Create(data)));

            if (IsActiveVessel(data))
            {
                Log.Debug("GameEventMediator.OnVesselModified - dispatching active vessel modified signal");
                TryAction(() => ActiveVesselModifiedSignal.Dispatch());
            }
        }


        private void OnVesselDestroyed(Vessel data)
        {
            Log.Debug("GameEventMediator.OnVesselDestroyed");

            TryAction(() => VesselDestroyedSignal.Dispatch(GameFactory.Create(data)));

            if (IsActiveVessel(data))
            {
                Log.Debug("GameEventMediator.OnVesselDestroyed - dispatching active vessel destroyed signal");
                TryAction(() => ActiveVesselDestroyedSignal.Dispatch());
            }
        }


        private void OnGameSceneLoadRequested(GameScenes data)
        {
            Log.Debug(() => typeof(GameEventMediator).Name + ".OnGameSceneLoadRequested " + data);
            TryAction(() => GameSceneLoadRequestedSignal.Dispatch(data));
        }


        private void OnScienceReceived(float data0, ScienceSubject data1, ProtoVessel data2, bool data3)
        {
            Log.Debug(() => typeof (GameEventMediator).Name + ".OnScienceReceived " + data0);
            TryAction(() => ScienceReceivedSignal.Dispatch(data0, data1, data2, data3));
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


        private static bool IsActiveVessel(Vessel data)
        {
            return ReferenceEquals(data, FlightGlobals.ActiveVessel);
        }
    }
}
