using strange.extensions.mediation.impl;
using UnityEngine;

namespace ScienceAlert.VesselContext.Gui
{
    public class ExperimentPopupMediator : Mediator
    {
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global

        [Inject] public ExperimentPopupView View { get; set; }

        [Inject] public SignalUpdateExperimentListEntryPopup UpdateSignal { get; set; }
        [Inject] public SignalCloseExperimentListEntryPopup CloseSignal { get; set; }

        private ExperimentListEntry _currentPopupEntry;
        private ExperimentPopupType _currentPopupType = ExperimentPopupType.Default;

        public override void OnRegister()
        {
            base.OnRegister();
            UpdateSignal.AddListener(OnUpdate);
            CloseSignal.AddListener(OnClose);

            View.WindowRect = new Rect(0f, 0f, 100f, 60f); // todo: properly calculate min size
        }


        public override void OnRemove()
        {
            UpdateSignal.RemoveListener(OnUpdate);
            CloseSignal.RemoveListener(OnClose);
            base.OnRemove();
        }


        private void OnUpdate(ExperimentListEntry entry, ExperimentPopupType popupType, Vector2 mouseLocation)
        {
            if (_currentPopupType != popupType || !_currentPopupEntry.Equals(entry))
                OpenPopup(entry, popupType, mouseLocation);
            else UpdatePopupLocation(mouseLocation);
        }


        private void OnClose()
        {
            ClosePopup();
        }


        private void ClosePopup()
        {
            View.enabled = false;
        }


        private void OpenPopup(ExperimentListEntry entry, ExperimentPopupType type, Vector2 location)
        {
            _currentPopupEntry = entry;
            _currentPopupType = type;

            View.RebuildDisplayData(entry, type);
            View.enabled = true;

            UpdatePopupLocation(location);
        }


        private void UpdatePopupLocation(Vector2 location)
        {
            var currentLocation = View.WindowRect;
            currentLocation.center = location;

            View.WindowRect = KSPUtil.ClampRectToScreen(currentLocation);
        }
    }
}
