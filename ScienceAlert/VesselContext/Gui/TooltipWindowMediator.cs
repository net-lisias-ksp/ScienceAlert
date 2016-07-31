using System;
using System.Linq;
using JetBrains.Annotations;
using ReeperCommon.Logging;
using strange.extensions.mediation.impl;
using ScienceAlert.UI;
using ScienceAlert.UI.ExperimentWindow;
using ScienceAlert.UI.TooltipWindow;
using ScienceAlert.VesselContext.Experiments;
using ScienceAlert.VesselContext.Experiments.Sensors;

// ReSharper disable MemberCanBePrivate.Global

namespace ScienceAlert.VesselContext.Gui
{
    // ReSharper disable once ClassNeverInstantiated.Global
    class TooltipWindowMediator : Mediator
    {
        [Inject] public TooltipWindowView View { get; set; }

        [Inject] public ExperimentIdentifierProvider IdentifierProvider { get; set; }

        [Inject] public ISensorStateCache SensorStateCache { get; set; }
        [Inject] public IAlertStateCache AlertStateCache { get; set; }

        [Inject] public SignalExperimentSensorStatusChanged StateChange { get; set; }

        [Inject] public SignalSetTooltip TooltipSignal { get; set; }

        [Inject] public SignalContextIsBeingDestroyed ContextDestroyedSignal { get; set; }

        private ExperimentAlertStatus[] _possibleAlertStatues;
        private ExperimentListItemView.Indicator _currentTooltip;
        private string _alertTooltipText = string.Empty;
        private ExperimentAlertStatus _alertTextStatus = ExperimentAlertStatus.None;


        public override void OnRegister()
        {
            base.OnRegister();
            Hide();

            TooltipSignal.AddListener(OnTooltip);
            StateChange.AddListener(OnSensorStateChanged);

            ContextDestroyedSignal.AddOnce(OnVesselContextDestroyed);
            _possibleAlertStatues =
                Enum.GetValues(typeof (ExperimentAlertStatus)).Cast<ExperimentAlertStatus>().ToArray();
        }


        public override void OnRemove()
        {
            TooltipSignal.RemoveListener(OnTooltip);
            StateChange.RemoveListener(OnSensorStateChanged);
            base.OnRemove();
        }


        private void Hide()
        {
            View.Visible = false;
            _currentTooltip = ExperimentListItemView.Indicator.None;
        }


        private void Show()
        {
            View.Visible = true;
        }


        private void OnTooltip([NotNull] IExperimentIdentifier identifier, ExperimentListItemView.Indicator type)
        {
            if (identifier == null) throw new ArgumentNullException("identifier");

            if (type == ExperimentListItemView.Indicator.None)
            {
                Hide();
                return;
            }

            _currentTooltip = type;
            View.SetTooltip(GetText(identifier, type));
            Show();
        }


        private void OnSensorStateChanged(SensorStatusChange sensorStatusChange)
        {
            if (_currentTooltip == ExperimentListItemView.Indicator.None) return;
            View.SetTooltip(GetText(IdentifierProvider.Get(sensorStatusChange.CurrentState.Experiment), _currentTooltip));
        }


        private string GetText(IExperimentIdentifier identifier, ExperimentListItemView.Indicator type)
        {
            ExperimentSensorState cachedState;

            switch (type)
            {
                case ExperimentListItemView.Indicator.Recovery:
                {
                    cachedState = SensorStateCache.GetCachedState(identifier);
                    return "Recovery: " + cachedState.RecoveryValue.ToString("F1");
                }
                case ExperimentListItemView.Indicator.Transmission:
                {
                    cachedState = SensorStateCache.GetCachedState(identifier);
                    return "Transmission: " + cachedState.TransmissionValue.ToString("F1");
                }
                case ExperimentListItemView.Indicator.Lab:
                {
                    cachedState = SensorStateCache.GetCachedState(identifier);
                    return "Lab Analysis: " + cachedState.LabValue.ToString("F1");
                }
                case ExperimentListItemView.Indicator.Alert:
                {
                    var alertState = AlertStateCache.GetStatus(identifier);

                    // avoid doing string manipulation on every mouse over frame by seeing if we can reuse the last tooltip text
                    if (string.IsNullOrEmpty(_alertTooltipText) || _alertTextStatus != alertState)
                    {
                        var activeAlerts =
                            _possibleAlertStatues.Where(possible => (alertState & possible) != 0).Select(activeState => activeState.ToString()).ToArray();

                        
                        _alertTooltipText = (activeAlerts.Length > 0 ? string.Join(",", activeAlerts) : ExperimentAlertStatus.None.ToString());
                        _alertTextStatus = alertState;
                    }

                    return _alertTooltipText;
                }
                default:
                    return "Unrecognized tooltip type: " + type;
            }
        }



        private void OnVesselContextDestroyed()
        {
            Log.Warning("Destroying tooltip window");
            Destroy(gameObject);
        }
    }
}
