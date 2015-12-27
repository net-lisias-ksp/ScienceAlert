using System;
using strange.extensions.command.impl;
using UnityEngine;

namespace ScienceAlert.VesselContext.Gui
{
    public class CommandSpawnExperimentStatusReportPopup : Command
    {
        private readonly GameObject _viewContainer;
        private readonly ExperimentStatusReport _statusReport;
        private readonly ExperimentView.PopupType _popupType;
        private readonly SignalDestroyExperimentReportPopup _destroyPopupSignal;

        public CommandSpawnExperimentStatusReportPopup(
            [Name(VesselContextKeys.GuiContainer)] GameObject viewContainer, 
            ExperimentStatusReport statusReport,
            ExperimentView.PopupType popupType,
            SignalDestroyExperimentReportPopup destroyPopupSignal)
        {
            if (viewContainer == null) throw new ArgumentNullException("viewContainer");
            if (destroyPopupSignal == null) throw new ArgumentNullException("destroyPopupSignal");
            _viewContainer = viewContainer;
            _statusReport = statusReport;
            _popupType = popupType;
            _destroyPopupSignal = destroyPopupSignal;
        }


        public override void Execute()
        {
            //if (_viewContainer.GetComponent<ExperimentStatusReportPopup>() != null)
            //{
            //    Log.Warning(typeof (CommandSpawnExperimentStatusReportPopup).Name +
            //                ": status popup wasn't properly destroyed");
            //    _destroyPopupSignal.Dispatch();
            //}

            var popupView = _viewContainer.AddComponent<ExperimentStatusReportPopup>();

            popupView.PopupType = _popupType;
            popupView.Status = _statusReport;
        }
    }
}
