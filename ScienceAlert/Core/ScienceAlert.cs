using System;
using System.Collections;
using System.Reflection;
using ReeperCommon.Containers;
using ReeperCommon.Extensions;
using ReeperKSP.Extensions;
using strange.extensions.signal.impl;
using UnityEngine;

namespace ScienceAlert.Core
{
    [KSPScenario(
        // ReSharper disable BitwiseOperatorOnEnumWithoutFlags
        ScenarioCreationOptions.AddToNewCareerGames |
        ScenarioCreationOptions.AddToNewScienceSandboxGames |
        ScenarioCreationOptions.AddToExistingCareerGames |
        ScenarioCreationOptions.AddToExistingScienceSandboxGames,
        new [] { GameScenes.FLIGHT })]
// ReSharper disable once UnusedMember.Global
// ReSharper disable once ClassNeverInstantiated.Global
    public class ScienceAlert : ScenarioModule
    {
        internal readonly Signal<ConfigNode> SaveSignal = new Signal<ConfigNode>();
        internal readonly Signal<ConfigNode> LoadSignal = new Signal<ConfigNode>();

        private GameObject _coreContextView;

// ReSharper disable once UnusedMember.Local
        private void OnPluginReloadRequested()
        {
            print("ScienceAlert: Reloading");
            _coreContextView.Do(Destroy);
        }


        public override void OnSave(ConfigNode node)
        {
            base.OnSave(node);

            try
            {
                SaveSignal.Do(s => s.Dispatch(node));
            }
            catch (Exception e)
            {
                Debug.LogError("ScienceAlert failed to save due to: " + e);
                Debug.Log("Contents of ConfigNode: " + node.ToString());
            }
        }


        public override void OnLoad(ConfigNode node)
        {
            StartCoroutine(Bootstrap(node));
        }


        private IEnumerator Bootstrap(ConfigNode onLoadConfig)
        {
            try
            {
                _coreContextView.IfNull(
                   () => _coreContextView = new GameObject("ScienceAlert.ContextView", typeof(BootstrapCore)));
            }
            catch (Exception e)
            {
                InitFailed(e.ToString());
                yield break;
            }


            yield return 0;


            try
            {
                LoadSignal.Do(s => s.Dispatch(onLoadConfig));
            }
            catch (Exception e)
            {
                InitFailed("Failed to dispatch load signal: " + e);
            }
        }


        private static void InitFailed(string message)
        {
            Debug.LogError("ScienceAlert failed to initialize due to: " + message);

            PopupDialog.SpawnPopupDialog(new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), "Initialization Failure",
                "ScienceAlert failed to initialize properly. It has been disabled.\nSee the log for details.", "OK",
                true, HighLogic.UISkin);

            Assembly.GetExecutingAssembly().DisablePlugin();
        }
    }
}
