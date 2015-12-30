using System;
using System.Linq;
using ReeperCommon.Containers;
using strange.extensions.mediation.impl;
using UnityEngine;

namespace ScienceAlert.VesselContext.Gui
{
// ReSharper disable once ClassNeverInstantiated.Global
    public class ExperimentPopupMediator : Mediator
    {
        [Inject] public ExperimentView View { get; set; }
        [Inject] public SignalSpawnExperimentReportPopup SpawnSignal { get; set; }
        [Inject] public SignalUpdateExperimentReportPopupLocation LocationSignal { get; set; }
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



        private void SpawnPopup(ExperimentStatusReport experimentStatusReport, ExperimentView.PopupType popupType, Vector2 location)
        {
            if (_currentType != ExperimentView.PopupType.None)
            {
                if (popupType == _currentType && _currentPopup.Value.Equals(experimentStatusReport))
                {
                    Log.Debug("position update " + Time.realtimeSinceStartup);
                    LocationSignal.Dispatch(location);
                    return; // same popup; already spawned
                }

                ClosePopup();
            }

            _currentPopup = experimentStatusReport.ToMaybe();
            _currentType = popupType;

            SpawnSignal.Dispatch(experimentStatusReport, popupType, location);
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
