using System;
using System.Collections.ObjectModel;
using ReeperCommon.Containers;
using ReeperCommon.Extensions;
using ReeperCommon.Logging;
using strange.extensions.context.api;
using strange.framework.api;
using ScienceAlert.Core;
using ScienceAlert.Game;
using ScienceAlert.UI.ExperimentWindow;
using ScienceAlert.UI.OptionsWindow;
using ScienceAlert.UI.TooltipWindow;
using ScienceAlert.VesselContext.Experiments;
using ScienceAlert.VesselContext.Experiments.Sensors;
using ScienceAlert.VesselContext.Experiments.Triggers;
using ScienceAlert.VesselContext.Gui;
using UnityEngine;
using Object = UnityEngine.Object;

namespace ScienceAlert.VesselContext
{
    class ActiveVesselContext : SignalContext
    {
        private IBinding _alertBinding = null;
        private IBinding _sharedSaveBinding = null;

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


            injectionBinder.Bind<SignalSaveGuiSettings>().ToSingleton();
            injectionBinder.Bind<SignalLoadGuiSettings>().ToSingleton();

 
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
            injectionBinder.Bind<SignalActiveVesselDestroyed>().ToSingleton();
            injectionBinder.Bind<SignalActiveVesselCrewModified>().ToSingleton();

            injectionBinder.Bind<SignalGameSceneLoadRequested>().ToSingleton();
            injectionBinder.Bind<SignalScienceReceived>().ToSingleton();
            injectionBinder.Bind<SignalDominantBodyChanged>().ToSingleton();

            injectionBinder.Bind<SignalApplicationQuit>().ToSingleton();
            injectionBinder.Bind<SignalGameTick>().ToSingleton();

            injectionBinder.Bind<SignalExperimentSensorStatusChanged>().ToSingleton();
            injectionBinder.Bind<SignalDeployExperiment>().ToSingleton();
            injectionBinder.Bind<SignalDeployExperimentFinished>().ToSingleton();
            injectionBinder.Bind<SignalSetTooltip>().ToSingleton();

            mediationBinder.BindView<OptionsWindowView>()
                .ToMediator<OptionsWindowMediator>();

            mediationBinder.BindView<ExperimentWindowView>()
                .ToMediator<ExperimentWindowMediator>();

            mediationBinder.BindView<TooltipWindowView>()
                .ToMediator<TooltipWindowMediator>();

            mediationBinder.BindView<OptionsListItemView>()
                .ToMediator<OptionsWindowListItemMediator>();

            injectionBinder.Bind<ITemporaryBindingFactory>().To<TemporaryBindingFactory>().ToSingleton();
            injectionBinder.Bind<IGameFactory>().Bind<KspFactory>().To<KspFactory>().ToSingleton();

            injectionBinder.Bind<IGameDatabase>().To<KspGameDatabase>().ToSingleton();
            injectionBinder.Bind<IScienceUtil>().To<KspScienceUtil>().ToSingleton();


            var stateChangeSignal = injectionBinder.GetInstance<SignalExperimentAlertChanged>();

            var stateCache = new AlertStateCache(injectionBinder.GetInstance<ReadOnlyCollection<ScienceExperiment>>(),
                injectionBinder.GetInstance<ExperimentIdentifierProvider>());

            stateChangeSignal.AddListener(stateCache.OnAlertStatusChange);

            injectionBinder.Bind<IAlertStateCache>().To(stateCache);

            SetupCommandBindings();

            //injectionBinder.ReflectAll();
        }



        private void SetupCommandBindings()
        {
            commandBinder.Bind<SignalStart>()
                .InSequence()
                .To<CommandConfigureGameEvents>()
                .To<CommandCreateVesselBindings>()
                .To<CommandCreateExperimentReportCalculator>()
                .To<CommandCreateExperimentTriggers>()
                .To<CommandCreateExperimentSensors>()
                .To<CommandCreateSensorUpdater>()
                .To<CommandCreateVesselGui>()
                .To<CommandDispatchLoadGuiSettingsSignal>()
                .To<CommandStartSensorUpdater>()
                .Once();

            commandBinder.Bind<SignalExperimentSensorStatusChanged>()
                .InSequence()
                .To<CommandLogSensorStatusUpdate>()
                .To<CommandUpdateAlertStatus>()
                .Pooled();

            commandBinder.Bind<SignalDeployExperiment>()
                .To<CommandDeployExperiment>()
                .Pooled();

            commandBinder.Bind<SignalContextIsBeingDestroyed>()
                .To<CommandDispatchSaveGuiSettingsSignal>()
                .Once();

            SetupCrossContextCommandBindings();
        }


        // Split into its own section because it's very important we UNBIND these when the context is destroyed
        private void SetupCrossContextCommandBindings()
        {
            _sharedSaveBinding = commandBinder.Bind<SignalSharedConfigurationSaving>()
                .To<CommandDispatchSaveGuiSettingsSignal>();

            _alertBinding = commandBinder.Bind<SignalExperimentAlertChanged>()
                .To<CommandPlayAlertSound>();
        }


        public override void OnRemove()
        {
            Log.Debug("Removing ActiveVesselContext cross context bindings");
            _sharedSaveBinding.Do(commandBinder.Unbind);
            _alertBinding.Do(commandBinder.Unbind);

            base.OnRemove();
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

                injectionBinder.GetInstance<SignalContextIsBeingDestroyed>().Do(s => s.Dispatch());
            }
            catch (Exception e)
            {
                Log.Error("Failed to signal destruction: " + e);
            } 
        }


        public void DestroyContext()
        {
            (contextView as GameObject).If(go => go != null).Do(Object.Destroy);
        }
    }
}
