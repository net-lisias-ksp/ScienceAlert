using System;
using System.Linq;
using ReeperCommon.Containers;
using strange.extensions.mediation.impl;

namespace ScienceAlert.VesselContext.Gui
{
    public class ExperimentPopupMediator : Mediator
    {
        [Inject] public ExperimentView View { get; set; }
        [Inject] public SignalSpawnExperimentReportPopup SpawnSignal { get; set; }
        [Inject] public SignalDestroyExperimentReportPopup DestroySignal { get; set; }

        private Maybe<ExperimentStatusReport> _currentPopup = Maybe<ExperimentStatusReport>.None;
        private ExperimentView.PopupType _currentType = ExperimentView.PopupType.None;

        public override void OnRegister()
        {
            Log.Debug("ExperimentPopupMediator.OnRegister");
            base.OnRegister();
            View.SpawnPopup.AddListener(SpawnPopup);
            View.ClosePopup.AddListener(ClosePopup);
        }



        public override void OnRemove()
        {
            base.OnRemove();
            View.SpawnPopup.RemoveListener(SpawnPopup);
            View.ClosePopup.RemoveListener(ClosePopup);
        }



        private void SpawnPopup(ExperimentStatusReport experimentStatusReport, ExperimentView.PopupType popupType)
        {
            if (_currentType != ExperimentView.PopupType.None)
            {
                if (popupType == _currentType && _currentPopup.Value.Equals(experimentStatusReport))
                    return; // same popup; already spawned

                ClosePopup();
            }

            _currentPopup = experimentStatusReport.ToMaybe();
            _currentType = popupType;

            SpawnSignal.Dispatch(experimentStatusReport, popupType);
        }



        private void ClosePopup()
        {
            if (_currentType == ExperimentView.PopupType.None) return; 

            try
            {
                DestroySignal.Dispatch();
                _currentPopup = Maybe<ExperimentStatusReport>.None;
                _currentType = ExperimentView.PopupType.None;
            }
            catch (Exception e)
            {
                Log.Error("Exception while destroying experiment popup: " + e);

                // todo: what do we do!? maybe signal plugin to terminate because something is horribly broken
            }
        }
    }
}
