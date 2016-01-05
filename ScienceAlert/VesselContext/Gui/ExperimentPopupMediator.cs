using strange.extensions.mediation.impl;
using UnityEngine;

namespace ScienceAlert.VesselContext.Gui
{
    public class ExperimentPopupMediator : Mediator
    {
        [Inject] public ExperimentPopupView View { get; set; }
        [Inject]
        public SignalUpdateExperimentListPopup UpdateSignal { get; set; }

        public override void OnRegister()
        {
            base.OnRegister();
            UpdateSignal.AddListener(OnUpdate);

            View.WindowRect = new Rect(0f, 0f, 100f, 60f); // todo: properly calculate min size
        }


        public override void OnRemove()
        {
            UpdateSignal.RemoveListener(OnUpdate);
            base.OnRemove();
        }


        private void OnUpdate(ExperimentSensorState status, ExperimentListView.PopupType popupType, Vector2 mouseLocation)
        {
            if (popupType == ExperimentListView.PopupType.None)
            {
                if (View.enabled) ClosePopup();
                return;
            }

            if (View.PopupType != popupType || !View.Status.Equals(status))
                OpenPopup(status, popupType, mouseLocation);

            UpdatePopupLocation(mouseLocation);
        }


        private void ClosePopup()
        {
            View.PopupType = ExperimentListView.PopupType.None;
            View.enabled = false;
        }


        private void OpenPopup(ExperimentSensorState report, ExperimentListView.PopupType type, Vector2 location)
        {
            View.PopupType = type;
            View.Status = report;
            View.enabled = true;
        }


        private void UpdatePopupLocation(Vector2 location)
        {
            View.SetLocation(location);
        }
    }
}
