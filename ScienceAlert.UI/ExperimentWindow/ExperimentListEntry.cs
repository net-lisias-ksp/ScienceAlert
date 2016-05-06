using System;
using ReeperCommon.Containers;
using ReeperCommon.Logging;
using strange.extensions.signal.impl;
using UnityEngine;
using UnityEngine.UI;

#pragma warning disable 649
#pragma warning disable 169

namespace ScienceAlert.UI.ExperimentWindow
{
    [DisallowMultipleComponent, Serializable]
    // ReSharper disable once UnusedMember.Global
    internal class ExperimentListEntry : MonoBehaviour
    {
        public readonly Signal<IExperimentEntry> Deploy = new Signal<IExperimentEntry>();

        [SerializeField] private Button _deployButton;
        [NonSerialized, HideInInspector] public IExperimentEntry Experiment;

        // ReSharper disable once UnusedMember.Global
        public void OnDeploy()
        {
            Experiment
                .IfNull(() => Log.Error("No experiment set for this button controller"))
                .Do(e => Deploy.Dispatch(e));
        }


        // ReSharper disable once UnusedMember.Global
        public bool IsEnabled
        {
            get { return _deployButton.interactable; }
            set { _deployButton.interactable = value; }
        }
    }
}
