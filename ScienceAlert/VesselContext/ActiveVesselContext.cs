using System;
using ReeperCommon.Containers;
using ReeperCommon.Extensions;
using ReeperCommon.Logging;
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
                .ToName(VesselContextKeys.ExperimentViewConfig);

            injectionBinder.Bind<ConfigNode>()
                .To(sharedConfig.VesselDebugViewConfig)
                .ToName(VesselContextKeys.VesselDebugViewConfig);

            injectionBinder.Bind<SignalSaveGuiSettings>().ToSingleton();
            injectionBinder.Bind<SignalLoadGuiSettings>().ToSingleton();

 

            // note to self: see how these are NOT cross context? That's because each ContextView
            // has its own GameEventView. This is done to avoid having to do any extra bookkeeping (of
            // removing events we've subscribed to) in the event that a ContextView is destroyed.
            // 
            // If we were to register these to cross context signals, those publishers might keep objects
            // designed for the current active vessel alive and away from the GC even when the rest of the
            // context was destroyed
            injectionBinder.Bind<SignalActiveVesselChanged>().ToSingleton();
            injectionBinder.Bind<SignalActiveVesselModified>().ToSingleton();

            injectionBinder.Bind<SignalActiveVesselDestroyed>().ToSingleton();
            injectionBinder.Bind<SignalCrewOnEva>().ToSingleton();
            injectionBinder.Bind<SignalCrewTransferred>().ToSingleton();
            injectionBinder.Bind<SignalGameSceneLoadRequested>().ToSingleton();
            injectionBinder.Bind<SignalScienceReceived>().ToSingleton();

            injectionBinder.Bind<SignalApplicationQuit>().ToSingleton();
            injectionBinder.Bind<SignalGameTick>().ToSingleton();

            injectionBinder.Bind<SignalUpdateExperimentListEntryPopup>().ToSingleton();
            injectionBinder.Bind<SignalCloseExperimentListEntryPopup>().ToSingleton();

            injectionBinder.Bind<SignalExperimentSensorStatusChanged>().ToSingleton();
            injectionBinder.Bind<SignalTriggerSensorStatusUpdate>().ToSingleton();
            injectionBinder.Bind<SignalDeployExperiment>().ToSingleton();

            injectionBinder.Bind<ExperimentListEntryFactory>().ToSingleton();

            mediationBinder.BindView<ExperimentListView>()
                .ToMediator<ExperimentListMediator>()
                .ToMediator<ExperimentListPopupMediator>();

            injectionBinder.Bind<ITemporaryBindingFactory>().To<TemporaryBindingFactory>().ToSingleton();
            injectionBinder.Bind<IGameFactory>().Bind<KspFactory>().To<KspFactory>().ToSingleton();

            injectionBinder.Bind<IGameDatabase>().To<KspGameDatabase>().ToSingleton();
            injectionBinder.Bind<IScienceUtil>().To<KspScienceUtil>().ToSingleton();

            injectionBinder.Bind<IScienceSubjectProvider>()
                .To<KspScienceSubjectProvider>().ToSingleton();

            SetupCommandBindings();

            injectionBinder.ReflectAll();
        }



        private void SetupCommandBindings()
        {
            commandBinder.Bind<SignalStart>()
                .InSequence()
                .To<CommandBindExperimentRuleTypes>()
                .To<CommandConfigureGameEvents>()
                .To<CommandCreateVesselBindings>()
                .To<CommandCreateExperimentReportCalculator>()
                .To<CommandCreateExperimentSensors>()
                .To<CommandCreateVesselGui>()
                .To<CommandDispatchLoadGuiSettingsSignal>()
                .To<CommandTriggerInitialSensorUpdates>()
                .Once();

            commandBinder.Bind<SignalExperimentSensorStatusChanged>()
                .To<CommandLogSensorStatusUpdate>();
            //    .Pooled();

            commandBinder.Bind<SignalDeployExperiment>()
                .To<CommandDeployExperiment>();

            commandBinder.Bind<SignalContextDestruction>()
                .To<CommandDispatchSaveGuiSettingsSignal>()
                .Once();

            commandBinder.Bind<SignalSharedConfigurationSaving>()
                .To<CommandDispatchSaveGuiSettingsSignal>();

            commandBinder.Bind<SignalCriticalShutdown>()
                .To<CommandCriticalShutdown>().Once();

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

        private void CurrentDomainOnUnhandledException(object sender, UnhandledExceptionEventArgs unhandledExceptionEventArgs)
        {
            throw new NotImplementedException();
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

                injectionBinder.GetInstance<SignalContextDestruction>().Do(s => s.Dispatch());
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
