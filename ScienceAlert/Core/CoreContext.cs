using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using ReeperCommon.Containers;
using ReeperCommon.FileSystem;
using ReeperCommon.FileSystem.Providers;
using ReeperCommon.Logging;
using ReeperCommon.Repositories;
using ReeperCommon.Serialization;
using ScienceAlert.Experiments;
using ScienceAlert.Game;
using ScienceAlert.Gui;
using strange.extensions.context.api;
using UnityEngine;

namespace ScienceAlert.Core
{
    public class CoreContext : SignalContext
    {
        public CoreContext(MonoBehaviour view)
            : base(view, ContextStartupFlags.MANUAL_MAPPING | ContextStartupFlags.MANUAL_LAUNCH)
        {
        }


        protected override void mapBindings()
        {
            base.mapBindings();

            MapCrossContextBindings();
            SetupCommandBindings();

            injectionBinder.GetInstance<ILog>().Normal("ScienceAlert is operating normally");
        }


        private void MapCrossContextBindings()
        {
            injectionBinder.Bind<ILog>().To(new DebugLog("ScienceAlert")).CrossContext();

            injectionBinder.Bind<Assembly>().To(Assembly.GetExecutingAssembly()).CrossContext();

            injectionBinder.Bind<SignalScenarioModuleLoad>().ToSingleton().CrossContext();
            injectionBinder.Bind<SignalScenarioModuleSave>().ToSingleton().CrossContext();

            injectionBinder.Bind<IUrlDirProvider>().To<KSPGameDataUrlDirProvider>().ToSingleton().CrossContext();
            injectionBinder.Bind<IUrlDir>().To(new KSPUrlDir(injectionBinder.GetInstance<IUrlDirProvider>().Get())).CrossContext();
            injectionBinder.Bind<IFileSystemFactory>().To<KSPFileSystemFactory>().ToSingleton().CrossContext();

            injectionBinder
                .Bind<ScenarioConfiguration>()
                .To<ScenarioConfiguration>().ToSingleton().CrossContext();

            injectionBinder
                .Bind<IGuiConfiguration>()
                .Bind<GuiConfiguration>()
                .To<GuiConfiguration>().ToSingleton().CrossContext();

            injectionBinder.Bind<SignalVesselChanged>().ToSingleton().CrossContext();
            injectionBinder.Bind<SignalVesselModified>().ToSingleton().CrossContext();
            injectionBinder.Bind<SignalVesselDestroyed>().ToSingleton().CrossContext();
            injectionBinder.Bind<SignalGameTick>().ToSingleton().CrossContext();

            //var activeVesselQuery = new ActiveVesselProvider();
            //injectionBinder.Bind<IActiveVesselProvider>().To(activeVesselQuery).CrossContext();

            //injectionBinder.GetInstance<SignalVesselChanged>().AddListener(activeVesselQuery.OnVesselChanged);
            //injectionBinder.GetInstance<SignalVesselDestroyed>().AddListener(activeVesselQuery.OnVesselDestroyed);
        }


        private void SetupCommandBindings()
        {
            commandBinder.Bind<SignalScenarioModuleLoad>()
                .InSequence()
                .To<CommandCreateGui>()
                .To<CommandLoadGuiConfiguration>()
                .To<CommandLoadConfiguration>()
                //.To<CommandCreateActiveVesselView>() // because we'll definitely have missed the initial OnVesselChanged by now
                .Once();


            commandBinder.Bind<SignalScenarioModuleSave>()
                .To<CommandSaveConfiguration>();


            commandBinder.Bind<SignalStart>()
                .InSequence()
                .To<CommandConfigureScenarioModule>()
                .To<CommandConfigureAssemblyDirectory>()
                .To<CommandConfigureResourceRepository>()
                .To<CommandConfigureSerializer>()
                .To<CommandConfigureGuiSkinsAndTextures>()
                .To<CommandConfigureGameEvents>()
                .Once();


            commandBinder.Bind<SignalDestroy>()
                .InSequence()
                .To<CommandSaveConfiguration>()
                .To<CommandSaveGuiConfiguration>()
                .Once();
        }


        public override void Launch()
        {
            base.Launch();
            injectionBinder.GetInstance<SignalStart>().Dispatch();
        }


        public override void OnRemove() // note to self: this won't be called for the first context, i.e. this one
        {
            try
            {
                Debug.Log("CoreContext.OnRemove");
                injectionBinder.GetInstance<SignalDestroy>().Dispatch();
            }
            catch (Exception e)
            {
                Debug.LogError("Exception while dispatching destroy signal: " + e);
            }
            
            base.OnRemove();
        }
    }
}
