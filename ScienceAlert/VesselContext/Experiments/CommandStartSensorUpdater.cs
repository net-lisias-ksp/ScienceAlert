using ReeperCommon.Containers;
using ReeperCommon.Logging;
using strange.extensions.command.impl;
using strange.extensions.context.api;
using UnityEngine;

namespace ScienceAlert.VesselContext.Experiments
{
    // ReSharper disable once ClassNeverInstantiated.Global
    class CommandStartSensorUpdater : Command
    {
        // ReSharper disable once MemberCanBePrivate.Global
        [Inject(ContextKeys.CONTEXT_VIEW)] public GameObject Context { get; set; }

        public override void Execute()
        {
            Context.GetComponent<ExperimentSensorUpdater>()
                .Do(updater => updater.enabled = true)
                .IfNull(() => Log.Error("Can't start sensor updater: it wasn't found in the context view"));
        }
    }
}
