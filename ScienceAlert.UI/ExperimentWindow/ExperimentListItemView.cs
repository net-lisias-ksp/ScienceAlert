using System;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using ReeperCommon.Containers;
using ReeperCommon.Extensions;
using ReeperCommon.Logging;
using strange.extensions.context.api;
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
    public class ExperimentListItemView : ManualRegistrationView
    {
        public enum Indicator
        {
            Alert,
            Recovery,
            Transmission,
            Lab,
            None
        }

        [SerializeField] private Button _deployButton;
        [SerializeField] private Text _text;
        [SerializeField] private IndicatorController _recoveryIndicator;
        [SerializeField] private IndicatorController _transmissionIndicator;
        [SerializeField] private IndicatorController _labIndicator;
        [SerializeField] private IndicatorController _alertIndicator;

        public readonly Signal<Indicator> ChangeTooltip = new Signal<Indicator>();
        public readonly Signal Deploy = new Signal();

        protected override void Start()
        {
            GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                .Where(type => typeof (Selectable).IsAssignableFrom(type.FieldType))
                .Where(fi => fi.GetValue(this) == null)
                .ToList()
                .ForEach(nullField => Log.Error("ExperimentListItemView." + nullField.Name + " is not set"));

            Identifier.IfNull(() => Log.Error("Identifier not set"));

            base.Start();

            if (Identifier.IsNull())
                return;

            _alertIndicator.MouseEvent.AddListener(e => OnIndicatorMouseEvent(e, Indicator.Alert));
            _recoveryIndicator.MouseEvent.AddListener(e => OnIndicatorMouseEvent(e, Indicator.Recovery));
            _transmissionIndicator.MouseEvent.AddListener(e => OnIndicatorMouseEvent(e, Indicator.Transmission));
            _labIndicator.MouseEvent.AddListener(e => OnIndicatorMouseEvent(e, Indicator.Lab));
        }


        protected override void OnDestroy()
        {
            try
            {
                _alertIndicator.MouseEvent.RemoveAllListeners();
                _recoveryIndicator.MouseEvent.RemoveAllListeners();
                _transmissionIndicator.MouseEvent.RemoveAllListeners();
                _labIndicator.MouseEvent.RemoveAllListeners();
            }
            finally
            {
                base.OnDestroy();
            }
        }


        private void Initialize([NotNull] IExperimentIdentifier identifier)
        {
            if (identifier == null) throw new ArgumentNullException("identifier");

            Identifier = identifier;
        }


        // UnityAction
        public void OnDeployButtonClicked()
        {
            Deploy.Dispatch();
        }



        private void OnIndicatorMouseEvent(bool entered, Indicator which)
        {
            if (!entered) which = Indicator.None;

            Log.Warning("ListItemView dispatching tooltip: " + which);
            ChangeTooltip.Dispatch(which);
        }


        public IExperimentIdentifier Identifier { get; private set; }

        // ReSharper disable once UnusedMember.Global
        public bool Enabled { set { _deployButton.interactable = value; } }


        public string Text { set { _text.text = value; } }

        public float RecoveryValue { set; private get; }
        public float TransmissionValue { set; private get; }
        public float LabValue { set; private get; }

        public bool AlertLit
        {
            set { _alertIndicator.isOn = value; }
        }


        public bool RecoveryValueLit
        {
            set { _recoveryIndicator.isOn = value; }
        }


        public bool TransmissionValueLit
        {
            set { _transmissionIndicator.isOn = value; }
        }

        public bool LabValueLit
        {
            set { _labIndicator.isOn = value; }
        }


        public static class Factory
        {
            public static ExperimentListItemView Create(
                [NotNull] ExperimentListItemView prefab, 
                [NotNull] IContext context,
                IExperimentIdentifier identifier)
            {
                if (prefab == null) throw new ArgumentNullException("prefab");
                if (context == null) throw new ArgumentNullException("context");

                ExperimentListItemView item = null;

                try
                {
                    item = Instantiate(prefab);
                    context.AddView(item);
                    item.Initialize(identifier);

                    return item;
                }
                catch (Exception e)
                {
                    context.RemoveView(item);
                    item.Do(Destroy);
                    throw;
                }
            }
        }
    }
}
