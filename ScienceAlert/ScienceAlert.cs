using System;
using System.Collections;
using System.Reflection;
using ReeperCommon.Logging;
using ScienceAlert.Annotations;
using ScienceAlert.Commands;
using ScienceAlert.Game;
using UnityEngine;

namespace ScienceAlert
{
    [KSPScenario((ScenarioCreationOptions) 120, GameScenes.FLIGHT), UsedImplicitly]
    public class ScienceAlert : ScenarioModule
    {
        [Persistent] private readonly Settings _settings = new Settings();

        private Core _core;
        private ICommand _disablePlugin;
        private ILog _log = new NothingLog();


        [UsedImplicitly]
        IEnumerator Start()
        {
            _log = LogFactory.Create(_settings.LogLevel);
            _disablePlugin = SetupTerminationCommand();


            _log.Debug("Waiting for dependencies to initialize");
            yield return StartCoroutine(WaitForDependencyInitialization());


            try
            {
                _log.Verbose("Initializing core...");
                _core = new Core(_settings, _log);
                _log.Verbose("Finishied initializing");
            }
            catch (Exception e)
            {
                _log.Error("Unhandled exception while constructing Core: " + e);
                _disablePlugin.Execute();
            }
        }


        [UsedImplicitly]
        private void Update()
        {
            try
            {
                _core.Update();
            }
            catch (Exception e)
            {
                _log.Error("Unhandled exception in Update: " + e);
                _disablePlugin.Execute();
            }
        }


        private static IEnumerator WaitForDependencyInitialization()
        {
            while (ResearchAndDevelopment.Instance == null || !FlightGlobals.ready)
                yield return 0;
        }


        private ICommand SetupTerminationCommand()
        {
            var disablePlugin = new DisablePluginCommand(new KspAssemblyLoader(new KspFactory()),
                Assembly.GetExecutingAssembly(), _log);

            var destroyScenarioModule = new DestroyUnityObjectCommand<Component>(GetComponent<ScienceAlert>());

            var informPlayer = new SpawnInformationalPopupCommand(
                "ScienceAlert has been disabled",
                "An unhandled exception has been encountered and ScienceAlert must be disabled. Check the log for details. If you encounter this problem, please create a bug report. Thanks!",
                "Okay", false, HighLogic.Skin);

            return new CompositeCommand(disablePlugin, destroyScenarioModule, informPlayer);
        }


        public override void OnSave(ConfigNode node) { ConfigNode.CreateConfigFromObject(this, node); }
        public override void OnLoad(ConfigNode node) { ConfigNode.LoadObjectFromConfig(this, node); }
    }
}
