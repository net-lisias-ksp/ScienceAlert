using System;
using ReeperCommon.Containers;
using ReeperCommon.Extensions;
using ScienceAlert.Core;
using ScienceAlert.Game;
using ScienceAlert.VesselContext.Experiments;
using ScienceAlert.VesselContext.Gui;
using strange.extensions.context.api;
using UnityEngine;

namespace ScienceAlert.VesselContext
{
    public class ActiveVesselContext : SignalContext
    {
        public ActiveVesselContext(MonoBehaviour view) : base(view, ContextStartupFlags.MANUAL_LAUNCH)
        {
        }


        protected override void mapBindings()
        {
            base.mapBindings();

            if (FlightGlobals.ActiveVessel.IsNull())
            {
                Log.Error("ActiveVessel context created when no active vessel exists");
                return;
            }



            var sharedConfig = injectionBinder.GetInstance<SharedConfiguration>();

            injectionBinder.Bind<ConfigNode>()
                .To(sharedConfig.ExperimentViewConfig)
                .ToName(VesselKeys.ExperimentViewConfig);

            injectionBinder.Bind<ConfigNode>()
                .ToValue(sharedConfig.VesselDebugViewConfig)
                .ToName(VesselKeys.VesselDebugViewConfig);

            injectionBinder.Bind<IExperimentRuleFactory>().To<ExperimentRuleFactory>().ToSingleton();

            injectionBinder.Bind<IExperimentRulesetProvider>().To<ExperimentRulesetProvider>().ToSingleton();
            injectionBinder.Bind<IExperimentSensorFactory>().To<ExperimentSensorFactory>().ToSingleton();

            injectionBinder.Bind<SignalSaveGuiSettings>().ToSingleton();
            injectionBinder.Bind<SignalLoadGuiSettings>().ToSingleton();
            injectionBinder.Bind<SignalSensorStateChanged>().ToSingleton();

            // note to self: see how these are NOT cross context? That's because each ContextView
            // has its own GameEventView. This is done to avoid having to do any extra bookkeeping (of
            // removing events we've subscribed to) in the event that a ContextView is destroyed.
            // 
            // If we were to register these to cross context signals, those publishers might keep objects
            // designed for the current active vessel alive and away from the GC even when the rest of the
            // context was destroyed
            injectionBinder.Bind<SignalVesselChanged>().ToSingleton();
            injectionBinder.Bind<SignalVesselModified>().ToSingleton();
            injectionBinder.Bind<SignalActiveVesselModified>().ToSingleton();
            injectionBinder.Bind<SignalVesselDestroyed>().ToSingleton();
            injectionBinder.Bind<SignalActiveVesselDestroyed>().ToSingleton();


            var gameFactory = injectionBinder.GetInstance<IGameFactory>();
            var activeVessel = new KspVessel(gameFactory, FlightGlobals.ActiveVessel);
            injectionBinder.Bind<IVessel>().ToValue(activeVessel);


            SetupCommandBindings();
        }



        private void SetupCommandBindings()
        {
            commandBinder.Bind<SignalStart>()
                .InSequence()
                .To<CommandConfigureGameEvents>()
                .To<CommandCreateRuleTypeBindings>()
                .To<CommandCreateVesselGui>()
                .To<CommandLoadGuiSettings>()
                .To<CommandCreateSensorUpdater>()
                .Once();

            commandBinder.Bind<SignalSensorStateChanged>()
                .To<CommandLogSensorStatusUpdate>();

            commandBinder.Bind<SignalDestroy>()
                .To<CommandSaveGuiSettings>()
                .Once();
        }


        public override void Launch()
        {
            base.Launch();

            Log.Debug("Launching ActiveVesselContext");

            try
            {
                injectionBinder.GetInstance<SignalStart>().Do(s => s.Dispatch());
            }
            catch (Exception e)
            {
                Log.Error("Error while launching ActiveVesselContext: " + e);
                SignalDestruction(true);
            }
        }


        public void SignalDestruction(bool destroyContextGo)
        {
            Log.Verbose("Signaling ActiveVesselContext destruction");



            try
            {
                if (destroyContextGo)
                {
                    DestroyContext();
                    return; // context bootstrapper will issue destruction signal
                }

                injectionBinder.GetInstance<SignalDestroy>().Do(s => s.Dispatch());
            }
            catch (Exception e)
            {
                Log.Error("Failed to signal destruction: " + e);
            } 
        }


        public void DestroyContext()
        {
            (contextView as GameObject).If(go => go != null).Do(UnityEngine.Object.Destroy);
        }
    }
}
