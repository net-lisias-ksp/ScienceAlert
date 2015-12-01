using System;
using System.Reflection;
using ReeperCommon.Extensions;
using ReeperCommon.Logging;
using ScienceAlert.Core;
using ScienceAlert.Experiments;
using strange.extensions.injector;
using strange.extensions.mediation.impl;

namespace ScienceAlert.Game
{
    public class GameEventMediator : Mediator
    {
        [Inject] public GameEventView View { get; set; }
        [Inject] public ILog Log { get; set; }
        [Inject] public IPopupDialogSpawner DialogSpawner { get; set; }

        [Inject] public SignalVesselChanged VesselChangedSignal { get; set; }
        [Inject] public SignalVesselModified VesselModifiedSignal { get; set; }
        [Inject] public SignalVesselDestroyed VesselDestroyedSignal { get; set; }

        [Inject] public SignalGameTick GameTickSignal { get; set; }


        public override void OnRegister()
        {
            base.OnRegister();
            View.VesselChanged.AddListener(OnVesselChanged);
            View.VesselModified.AddListener(OnVesselModified);
            View.VesselDestroyed.AddListener(OnVesselDestroyed);
            View.GameTick.AddListener(OnGameTick);
        }


        public override void OnRemove()
        {
            base.OnRemove();
            View.VesselChanged.RemoveListener(OnVesselChanged);
            View.VesselModified.RemoveListener(OnVesselModified);
            View.VesselDestroyed.RemoveListener(OnVesselDestroyed);
            View.GameTick.RemoveListener(OnGameTick);
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
            TryAction(() => VesselChangedSignal.Dispatch(new KspVessel(data)));
        }


        private void OnVesselModified(Vessel data)
        {
            TryAction(() => VesselModifiedSignal.Dispatch(new KspVessel(data)));
        }


        private void OnVesselDestroyed(Vessel data)
        {
            TryAction(() => VesselDestroyedSignal.Dispatch(new KspVessel(data)));
        }


        private void OnGameTick()
        {
            try
            {
                GameTickSignal.Dispatch();
            }
            catch (Exception e)
            {
                Log.Error("Encountered exception during update tick: " + e);

                // whatever just happened will now likely spam the log so we're going to have to shut down...
                // unfortunately we can't really trust the binder now to dispatch events so preventing further
                // spam and disabling the plugin is the best thing to be done

                DialogSpawner.CriticalError("ScienceAlert has experienced a problem! Details are in the log.");
                Assembly.GetExecutingAssembly().DisablePlugin();
                View.GameTick.RemoveListener(OnGameTick);
            }
        }
    }
}
