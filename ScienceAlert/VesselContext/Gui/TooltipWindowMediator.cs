using System;
using JetBrains.Annotations;
using ReeperCommon.Extensions;
using ReeperCommon.Logging;
using strange.extensions.mediation.impl;
using ScienceAlert.UI;
using ScienceAlert.UI.ExperimentWindow;
using ScienceAlert.UI.TooltipWindow;
using ScienceAlert.VesselContext.Experiments;
// ReSharper disable MemberCanBePrivate.Global

namespace ScienceAlert.VesselContext.Gui
{
    // ReSharper disable once ClassNeverInstantiated.Global
    class TooltipWindowMediator : Mediator
    {
        [Inject] public TooltipWindowView View { get; set; }

        [Inject] public ExperimentStateCache StateCache { get; set; }
        [Inject] public SignalExperimentSensorStatusChanged StateChange { get; set; }

        [Inject] public SignalSetTooltip TooltipSignal { get; set; }

        private ExperimentWindowView.ExperimentIndicatorTooltipType _currentTooltip;

        public override void OnRegister()
        {
            base.OnRegister();
            Hide();

            TooltipSignal.AddListener(OnTooltip);
            StateChange.AddListener(OnSensorStateChanged);
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
            _currentTooltip = ExperimentWindowView.ExperimentIndicatorTooltipType.None;
        }


        private void Show()
        {
            View.Visible = true;

            //View.transform.root.gameObject.PrintComponents(new DebugLog("Root"));
        }


        private void OnTooltip([NotNull] IExperimentIdentifier identifier, ExperimentWindowView.ExperimentIndicatorTooltipType type)
        {
            if (identifier == null) throw new ArgumentNullException("identifier");

            if (type == ExperimentWindowView.ExperimentIndicatorTooltipType.None)
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
            if (_currentTooltip == ExperimentWindowView.ExperimentIndicatorTooltipType.None) return;
            View.SetTooltip(GetText(new KspExperimentIdentifier(sensorStatusChange.NewState.Experiment), _currentTooltip));
        }


        private string GetText(IExperimentIdentifier identifier, ExperimentWindowView.ExperimentIndicatorTooltipType type)
        {
            var cachedState = StateCache.GetCachedState(identifier);

            switch (type)
            {
                case ExperimentWindowView.ExperimentIndicatorTooltipType.Collection:
                    return "Collection: " + cachedState.CollectionValue.ToString("F1");
                case ExperimentWindowView.ExperimentIndicatorTooltipType.Transmission:
                    return "Transmission: " + cachedState.TransmissionValue.ToString("F1");
                case ExperimentWindowView.ExperimentIndicatorTooltipType.Lab:
                    return "Lab Analysis: " + cachedState.LabValue.ToString("F1");
                default:
                    throw new NotImplementedException(type.ToString());
            }
        }
    }
}
