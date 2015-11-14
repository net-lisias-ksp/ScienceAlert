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


        public override void OnAwake()
        {
            base.OnAwake();

// ReSharper disable once ObjectCreationAsStatement
            new GameObject("ScienceAlert.ContextView", typeof (BootstrapCore));
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
