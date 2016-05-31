using System;
using System.Linq;
using System.Reflection;
using ReeperCommon.Logging;
using strange.extensions.signal.impl;
using UnityEngine;
using UnityEngine.UI;
// ReSharper disable UnusedMember.Global

#pragma warning disable 649
#pragma warning disable 169

namespace ScienceAlert.UI.ExperimentWindow
{
    [DisallowMultipleComponent, Serializable]
    // ReSharper disable once UnusedMember.Global
    internal class ExperimentListEntry : MonoBehaviour
    {
        public readonly Signal<IExperimentIdentifier> Deploy = new Signal<IExperimentIdentifier>();

        public readonly Signal<IExperimentIdentifier, ExperimentWindowView.ExperimentIndicatorTooltipType> MousedOverIndicator =
            new Signal<IExperimentIdentifier, ExperimentWindowView.ExperimentIndicatorTooltipType>();

        public IExperimentIdentifier Identifier;

        [SerializeField] private Button _deployButton;
        [SerializeField] private Text _text;
        [SerializeField] private Toggle _recoveryIndicatorLight;
        [SerializeField] private Toggle _transmissionIndicatorLight;
        [SerializeField] private Toggle _labIndicatorLight;
        [SerializeField] private Toggle _alertLight;

        private void Start()
        {
            GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                .Where(type => typeof (Selectable).IsAssignableFrom(type.FieldType))
                .Where(fi => fi.GetValue(this) == null)
                .ToList()
                .ForEach(nullField => Log.Error("ExperimentListEntry." + nullField.Name + " is not set"));
        }


        // ReSharper disable once UnusedMember.Global
        public void OnDeploy()
        {
            Deploy.Dispatch(Identifier);
        }


        // ReSharper disable once UnusedMember.Global
        public bool Enabled { set { _deployButton.interactable = value; } }


        public string Text { set { _text.text = value; } }

        public float RecoveryValue { set; private get; }
        public float TransmissionValue { set; private get; }
        public float LabValue { set; private get; }

        public bool AlertLit
        {
            set { _alertLight.isOn = value; }
        }


        public bool RecoveryValueLit
        {
            set { _recoveryIndicatorLight.isOn = value; }
        }


        public bool TransmissionValueLit
        {
            set { _transmissionIndicatorLight.isOn = value; }
        }

        public bool LabValueLit
        {
            set { _labIndicatorLight.isOn = value; }
        }


        private void CloseTooltip()
        {
            MousedOverIndicator.Dispatch(Identifier, ExperimentWindowView.ExperimentIndicatorTooltipType.None);
        }


        // UnityAction
        public void MouseEnterAlertIndicator()
        {
            MousedOverIndicator.Dispatch(Identifier, ExperimentWindowView.ExperimentIndicatorTooltipType.Alert);
        }


        // UnityAction
        public void MouseExitAlertIndicator()
        {
            CloseTooltip();
        }


        // UnityAction
        public void MouseEnterRecoveryIndicator()
        {
            MousedOverIndicator.Dispatch(Identifier, ExperimentWindowView.ExperimentIndicatorTooltipType.Recovery);
        }


        // UnityAction
        public void MouseExitRecoveryIndicator()
        {
            CloseTooltip();
        }


        // UnityAction
        public void MouseEnterTransmissionIndicator()
        {
            MousedOverIndicator.Dispatch(Identifier, ExperimentWindowView.ExperimentIndicatorTooltipType.Transmission);
        }


        // UnityAction
        public void MouseExitTransmissionIndicator()
        {
            CloseTooltip();
        }


        // UnityAction
        public void MouseEnterLabIndicator()
        {
            MousedOverIndicator.Dispatch(Identifier, ExperimentWindowView.ExperimentIndicatorTooltipType.Lab);
        }


        // UnityAction
        public void MouseExitLabIndicator()
        {
            CloseTooltip();
        }
    }
}
