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

            mediationBinder.BindView<ApplicationLauncherView>().ToMediator<ApplicationLauncherMediator>();
            //mediationBinder.BindView<AlertPanelView>().ToMediator<AlertPanelMediator>();

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
                .Bind<CoreConfiguration>()
                .Bind<IGuiSettings>()
                .To<CoreConfiguration>().ToSingleton().CrossContext();
        }


        private void SetupCommandBindings()
        {
            commandBinder.Bind<SignalScenarioModuleLoad>()
                .To<CommandLoadConfiguration>();

            commandBinder.Bind<SignalScenarioModuleSave>()
                .To<CommandSaveConfiguration>();

            commandBinder.Bind<SignalStart>()
                .InSequence()
                .To<CommandConfigureScenarioModule>() // it's very important we not miss those OnSave/OnLoads
                .To<CommandConfigureAssemblyDirectory>()
                .To<CommandConfigureResourceRepository>()
                .To<CommandConfigureGuiSkinsAndTextures>()
                .To<CommandCreateGui>()
                .Once();
        }


        public override void Launch()
        {
            base.Launch();
            injectionBinder.GetInstance<SignalStart>().Dispatch();
        }
    }
}
