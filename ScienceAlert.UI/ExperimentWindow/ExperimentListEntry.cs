using System;
using System.Linq;
using System.Reflection;
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
        public readonly Signal<IExperimentIdentifier> Deploy = new Signal<IExperimentIdentifier>();

        public readonly Signal<IExperimentIdentifier, ExperimentWindowView.ExperimentIndicatorTooltipType> MousedOverIndicator =
            new Signal<IExperimentIdentifier, ExperimentWindowView.ExperimentIndicatorTooltipType>();

        public IExperimentIdentifier Identifier;

        [SerializeField] private Button _deployButton;
        [SerializeField] private Text _text;
        [SerializeField] private Toggle _collectionIndicatorLight;
        [SerializeField] private Toggle _transmissionIndicatorLight;
        [SerializeField] private Toggle _labIndicatorLight;

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

        public float CollectionValue { set; private get; }
        public float TransmissionValue { set; private get; }
        public float LabValue { set; private get; }


        public bool CollectionAlertLit
        {
            set { _collectionIndicatorLight.isOn = value; }
        }


        public bool TransmissionAlertLit
        {
            set { _transmissionIndicatorLight.isOn = value; }
        }

        public bool LabAlertLit
        {
            set { _labIndicatorLight.isOn = value; }
        }


        private void CloseTooltip()
        {
            MousedOverIndicator.Dispatch(Identifier, ExperimentWindowView.ExperimentIndicatorTooltipType.None);
        }


        public void MouseEnterCollectionIndicator()
        {
            Log.Normal("Mouse entered collection indicator");
            MousedOverIndicator.Dispatch(Identifier, ExperimentWindowView.ExperimentIndicatorTooltipType.Collection);
        }


        public void MouseExitCollectionIndicator()
        {
            Log.Normal("Mouse exited collection indicator");
            MousedOverIndicator.Dispatch(Identifier, ExperimentWindowView.ExperimentIndicatorTooltipType.None);
        }


        public void MouseEnterTransmissionIndicator()
        {
            Log.Normal("Mouse entered transmission indicator");
            MousedOverIndicator.Dispatch(Identifier, ExperimentWindowView.ExperimentIndicatorTooltipType.Transmission);
        }


        public void MouseExitTransmissionIndicator()
        {
            Log.Normal("Mouse exited transmission indicator");
            MousedOverIndicator.Dispatch(Identifier, ExperimentWindowView.ExperimentIndicatorTooltipType.None);
        }


        public void MouseEnterLabIndicator()
        {
            Log.Normal("Mouse entered lab indicator");
            MousedOverIndicator.Dispatch(Identifier, ExperimentWindowView.ExperimentIndicatorTooltipType.Lab);
        }


        public void MouseExitLabIndicator()
        {
            Log.Normal("Mouse exited lab indicator");
            MousedOverIndicator.Dispatch(Identifier, ExperimentWindowView.ExperimentIndicatorTooltipType.None);
        }
    }
}
