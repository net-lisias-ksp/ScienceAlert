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


        public override void OnRegister()
        {
            base.OnRegister();
            View.VesselChanged.AddListener(OnVesselChanged);
            View.VesselModified.AddListener(OnVesselModified);
            View.VesselDestroyed.AddListener(OnVesselDestroyed);
        }


        public override void OnRemove()
        {
            base.OnRemove();
            View.VesselChanged.RemoveListener(OnVesselChanged);
            View.VesselModified.RemoveListener(OnVesselModified);
            View.VesselDestroyed.RemoveListener(OnVesselDestroyed);
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


        private static bool IsActiveVessel(Vessel data)
        {
            return ReferenceEquals(data, FlightGlobals.ActiveVessel);
        }
    }
}
