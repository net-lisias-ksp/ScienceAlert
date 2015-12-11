using System;
using ReeperCommon.Containers;
using ReeperCommon.Extensions;
using ScienceAlert.Core;
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

            injectionBinder.Bind<IVessel>().To(new KspVessel(FlightGlobals.ActiveVessel));

            var sharedConfig = injectionBinder.GetInstance<SharedConfiguration>();

            injectionBinder.Bind<ConfigNode>()
                .To(sharedConfig.ExperimentViewConfig)
                .ToName(VesselKeys.ExperimentViewConfig);

            injectionBinder.Bind<ConfigNode>()
                .ToValue(sharedConfig.VesselDebugViewConfig)
                .ToName(VesselKeys.VesselDebugViewConfig);


            injectionBinder.Bind<SignalSaveGuiSettings>().ToSingleton();
            injectionBinder.Bind<SignalLoadGuiSettings>().ToSingleton();

            SetupCommandBindings();
        }



        private void SetupCommandBindings()
        {
            commandBinder.Bind<SignalStart>()
                .InSequence()
                .To<CommandCreateVesselGui>()
                .To<CommandLoadGuiSettings>()
                .Once();

            commandBinder.Bind<SignalDestroy>()
                .To<CommandSaveGuiSettings>()
                .To<CommandDestroyActiveVesselContext>()
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
                (contextView as GameObject).If(go => go != null).Do(UnityEngine.Object.Destroy);
            }
        }


        public void SignalDestruction()
        {
            Log.Verbose("Signaling ActiveVesselContext destruction");

            try
            {
                injectionBinder.GetInstance<SignalDestroy>().Do(s => s.Dispatch());
            }
            catch (Exception e)
            {
                Log.Error("Failed to signal destruction: " + e);
            }
        }
    }
}
