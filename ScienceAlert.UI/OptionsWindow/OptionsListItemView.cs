using System;
using JetBrains.Annotations;
using ReeperCommon.Logging;
using strange.extensions.context.api;
using strange.extensions.signal.impl;
using UnityEngine;
using UnityEngine.UI;

#pragma warning disable 649

// ReSharper disable UnusedMember.Global
#pragma warning disable 169

namespace ScienceAlert.UI.OptionsWindow
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class OptionsListItemView : ManualRegistrationView
    {
        [SerializeField] private Text _headerText;
        [SerializeField] private Toggle _enableAlertsToggle;
        [SerializeField] private Toggle _stopWarpToggle;
        [SerializeField] private Toggle _recoveryToggle;
        [SerializeField] private Toggle _transmissionToggle;
        [SerializeField] private Toggle _labToggle;
        [SerializeField] private Toggle _soundToggle;
        [SerializeField] private Toggle _animationToggle;
        [SerializeField] private Slider _subjectThresholdSlider;
        [SerializeField] private Slider _minimumThresholdSlider;

        public readonly Signal<bool> EnableAlertsSignal = new Signal<bool>();
        public readonly Signal<bool> StopWarpOnAlertSignal = new Signal<bool>();
        public readonly Signal<bool> RecoveryAlertSignal = new Signal<bool>();
        public readonly Signal<bool> TransmissionAlertSignal = new Signal<bool>();
        public readonly Signal<bool> LabAlertSignal = new Signal<bool>();
        public readonly Signal<bool> SoundOnAlertSignal = new Signal<bool>();
        public readonly Signal<bool> AnimationOnAlertSignal = new Signal<bool>();
        public readonly Signal<float> SubjectResearchThresholdSignal = new Signal<float>();
        public readonly Signal<float> ReportMinimumThresholdSignal = new Signal<float>();

        private void Initialize(OptionDisplayStatus status)
        {
            Experiment = status.Experiment;
            AlertsEnabled = status.AlertsEnabled;
            RecoveryAlerts = status.RecoveryAlerts;
            TransmissionAlerts = status.TransmissionAlerts;
            LabAlerts = status.LabAlerts;
            AnimationOnAlert = status.AnimationOnAlert;
            SoundOnAlert = status.SoundOnAlert;
            SubjectResearchThreshold = status.SubjectResearchThreshold;
            ReportMinimumThreshold = status.ReportMinimumThreshold;
            Title = status.ExperimentTitle;
        }

        protected override void Start()
        {
            Log.Warning("OptionsListItemView.Start");
            base.Start();
        }

        public IExperimentIdentifier Experiment { get; private set; }

        public string Title
        {
            get { return _headerText.text; }
            set { _headerText.text = value; }
        }

        public bool AlertsEnabled
        {
            get { return _enableAlertsToggle.isOn; }
            set { _enableAlertsToggle.isOn = value; }
        }

        public bool StopWarp
        {
            get { return _stopWarpToggle.isOn; }
            set { _stopWarpToggle.isOn = value; }
        }

        public bool RecoveryAlerts
        {
            get { return _recoveryToggle.isOn; }
            set { _recoveryToggle.isOn = value; }
        }

        public bool TransmissionAlerts
        {
            get { return _transmissionToggle.isOn; }
            set { _transmissionToggle.isOn = value; }
        }

        public bool LabAlerts
        {
            get { return _labToggle.isOn; }
            set { _labToggle.isOn = value; }
        }

        public bool SoundOnAlert
        {
            get { return _soundToggle.isOn; }
            set { _soundToggle.isOn = value; }
        }

        public bool AnimationOnAlert
        {
            get { return _animationToggle.isOn; }
            set { _animationToggle.isOn = value; }
        }

        public float SubjectResearchThreshold
        {
            get { return _subjectThresholdSlider.value; }
            set { _subjectThresholdSlider.value = value; }
        }

        public float ReportMinimumThreshold
        {
            get { return _minimumThresholdSlider.value; }
            set { _minimumThresholdSlider.value = value; }
        }

        // UnityAction
        public void OnEnableAlertsToggled(bool tf) { EnableAlertsSignal.Dispatch(tf); }


        // UnityAction
        public void OnStopWarpOnAlertToggled(bool tf)
        {
            StopWarpOnAlertSignal.Dispatch(tf);
        }
  


        // UnityAction
        public void OnRecoveryAlertToggled(bool tf)
        {
            RecoveryAlertSignal.Dispatch(tf);
        }


        // UnityAction
        public void OnTransmissionAlertToggled(bool tf)
        {
            TransmissionAlertSignal.Dispatch(tf);
        }


        // UnityAction
        public void OnLabAlertToggled(bool tf)
        {
            LabAlertSignal.Dispatch(tf);
        }


        // UnityAction
        public void OnSoundOnAlertToggled(bool tf)
        {
            SoundOnAlertSignal.Dispatch(tf);
        }


        // UnityAction
        public void OnAnimationOnAlertToggled(bool tf)
        {
            AnimationOnAlertSignal.Dispatch(tf);
        }


        // UnityAction
        public void OnSubjectResearchThresholdChanged(float newValue)
        {
            SubjectResearchThresholdSignal.Dispatch(newValue);
        }

        // UnityAction
        public void OnReportScienceValueIgnoreThresholdChanged(float newValue)
        {
            ReportMinimumThresholdSignal.Dispatch(newValue);
        }

        public static class Factory
        {
            public static OptionsListItemView Create(
                [NotNull] OptionsListItemView prefab, [NotNull] IContext context, OptionDisplayStatus status)
            {
                if (prefab == null) throw new ArgumentNullException("prefab");
                if (context == null) throw new ArgumentNullException("context");
                if (status.Experiment == null)
                    throw new ArgumentException("must specify a valid experiment identifier");

                var newEntry = Instantiate(prefab);

                newEntry.Initialize(status);
                
                context.AddView(newEntry);
                
                return newEntry;
            }
        }
    }
}
