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
        private readonly IEnumerable<IExperimentSensorMonitor> _monitors;

        public CommandCreateExperimentSensorMonitorUpdater(
            [Name(ContextKeys.CONTEXT_VIEW)] GameObject vesselContextView,
            IEnumerable<IExperimentSensorMonitor> monitors)
        {
            if (vesselContextView == null) throw new ArgumentNullException("vesselContextView");
            if (monitors == null) throw new ArgumentNullException("monitors");
            _vesselContextView = vesselContextView;
            _monitors = monitors;
        }

        public override void Execute()
        {
            var updater = _vesselContextView.AddComponent<ExperimentSensorMonitorUpdater>();

            injectionBinder.injector.Inject(updater, false);
        }
    }
}
