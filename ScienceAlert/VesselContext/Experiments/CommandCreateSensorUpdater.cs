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

            injectionBinder.Bind<ISensorStateCache>().To(updater);

            updater.enabled = false; // don't begin running it just yet, we'll need to initialize the GUI too which might depend on ISensorStateCache
        }
    }
}
