using System;
using System.Collections.Generic;
using strange.extensions.command.impl;
using strange.extensions.context.api;
using UnityEngine;

namespace ScienceAlert.VesselContext.Experiments
{
// ReSharper disable once ClassNeverInstantiated.Global
    public class CommandCreateExperimentSensorMonitorUpdater : Command
    {
        private readonly GameObject _vesselContextView;

        public CommandCreateExperimentSensorMonitorUpdater(
            [Name(ContextKeys.CONTEXT_VIEW)] GameObject vesselContextView)
        {
            if (vesselContextView == null) throw new ArgumentNullException("vesselContextView");
            _vesselContextView = vesselContextView;
        }

        public override void Execute()
        {
            var updater = _vesselContextView.AddComponent<ExperimentSensorMonitorUpdater>();

            injectionBinder.injector.Inject(updater, false);
        }

        //[Inject(ContextKeys.CONTEXT_VIEW)]
        //public GameObject VesselContextView { get; set; }

        //[Inject] public IEnumerable<IExperimentSensorMonitor> Monitors { get; set; }

        //public override void Execute()
        //{
        //    var updater = VesselContextView.AddComponent<ExperimentSensorMonitorUpdater>();

        //    injectionBinder.injector.Inject(updater, false);
        //}
    }
}
