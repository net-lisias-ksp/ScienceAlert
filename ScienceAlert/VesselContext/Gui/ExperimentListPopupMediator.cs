using System;
using strange.extensions.mediation.impl;
using UnityEngine;

namespace ScienceAlert.VesselContext.Gui
{
// ReSharper disable once ClassNeverInstantiated.Global
    public class ExperimentListPopupMediator : Mediator
    {
        [Inject] public ExperimentListView View { get; set; }
        [Inject] public SignalUpdateExperimentListEntryPopup UpdateSignal { get; set; }
        [Inject] public SignalCloseExperimentListEntryPopup CloseSignal { get; set; }

        public override void OnRegister()
        {
            base.OnRegister();
            View.ExperimentPopup.AddListener(UpdatePopup);
            View.CloseExperimentPopup.AddListener(ClosePopup);
        }




        public override void OnRemove()
        {
            base.OnRemove();
            View.ExperimentPopup.RemoveListener(UpdatePopup);
            View.CloseExperimentPopup.RemoveListener(ClosePopup);
        }



        private void UpdatePopup(ExperimentListEntry entry, ExperimentPopupType popupType, Vector2 location)
        {
            UpdateSignal.Dispatch(entry, popupType, location);
        }


        private void ClosePopup()
        {
            CloseSignal.Dispatch();
        }
    }
}
