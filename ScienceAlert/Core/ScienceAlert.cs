using ReeperCommon.Containers;
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

        public override void OnAwake()
        {
            base.OnAwake();

            _coreContextView = new GameObject("ScienceAlert.ContextView", typeof(BootstrapCore));
        }


// ReSharper disable once UnusedMember.Local
        private void OnPluginReloadRequested()
        {
            print("ScienceAlert: Reloading");
            _coreContextView.Do(Destroy);
        }


        public override void OnSave(ConfigNode node)
        {
            base.OnSave(node);
            SaveSignal.Dispatch(node);
        }


        public override void OnLoad(ConfigNode node)
        {
            base.OnLoad(node);
            LoadSignal.Dispatch(node);
        }
    }
}
