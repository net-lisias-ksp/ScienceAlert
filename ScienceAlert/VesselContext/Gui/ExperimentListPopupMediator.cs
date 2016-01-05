using strange.extensions.mediation.impl;
using UnityEngine;

namespace ScienceAlert.VesselContext.Gui
{
// ReSharper disable once ClassNeverInstantiated.Global
    public class ExperimentListPopupMediator : Mediator
    {
        [Inject] public ExperimentListView View { get; set; }
        [Inject] public SignalUpdateExperimentListPopup UpdateSignal { get; set; }

        public override void OnRegister()
        {
            Log.Debug("ExperimentListPopupMediator.OnRegister");
            base.OnRegister();
            View.ExperimentPopup.AddListener(UpdatePopup);
        }



        public override void OnRemove()
        {
            base.OnRemove();
            View.ExperimentPopup.RemoveListener(UpdatePopup);
        }



        private void UpdatePopup(ExperimentSensorState experimentSensorState, ExperimentListView.PopupType popupType, Vector2 location)
        {
            UpdateSignal.Dispatch(experimentSensorState, popupType, location);
        }
    }
}
