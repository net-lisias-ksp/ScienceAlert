using System;
using strange.extensions.command.impl;
using strange.extensions.context.api;
using UnityEngine;

namespace ScienceAlert.VesselContext.Experiments
{
// ReSharper disable once ClassNeverInstantiated.Global
    class CommandCreateSensorUpdater : Command
    {
        private readonly GameObject _vesselContextView;

        public CommandCreateSensorUpdater([Name(ContextKeys.CONTEXT_VIEW)] GameObject vesselContextView)
        {
            if (vesselContextView == null) throw new ArgumentNullException("vesselContextView");
            _vesselContextView = vesselContextView;
        }


        public override void Execute()
        {
            var updater = _vesselContextView.AddComponent<ExperimentSensorUpdater>();

            injectionBinder.injector.Inject(updater, false);
        }
    }
}
