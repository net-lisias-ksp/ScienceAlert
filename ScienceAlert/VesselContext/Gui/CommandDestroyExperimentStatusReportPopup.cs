using System;
using ReeperCommon.Containers;
using strange.extensions.command.impl;
using UnityEngine;

namespace ScienceAlert.VesselContext.Gui
{
// ReSharper disable once ClassNeverInstantiated.Global
    public class CommandDestroyExperimentStatusReportPopup : Command
    {
        private readonly GameObject _viewContainer;

        public CommandDestroyExperimentStatusReportPopup([Name(VesselContextKeys.GuiContainer)] GameObject viewContainer)
        {
            if (viewContainer == null) throw new ArgumentNullException("viewContainer");
            _viewContainer = viewContainer;
        }


        public override void Execute()
        {
            _viewContainer.GetComponent<ExperimentStatusReportPopup>()
                .IfNull(() => Log.Warning("Cannot destroy experiment popup: does not exist"))
                .Do(UnityEngine.Object.Destroy)
                .Do(p => Log.Debug(() => "Destroyed experiment popup"));
        }
    }
}
