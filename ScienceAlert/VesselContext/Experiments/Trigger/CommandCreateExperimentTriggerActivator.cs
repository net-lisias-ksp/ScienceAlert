using System;
using ReeperCommon.Logging;
using strange.extensions.command.impl;

namespace ScienceAlert.VesselContext.Experiments.Trigger
{
// ReSharper disable once UnusedMember.Global
    class CommandCreateExperimentTriggerActivator : Command
    {
        private readonly SignalDeployExperiment _deploySignal;

        public CommandCreateExperimentTriggerActivator(SignalDeployExperiment deploySignal)
        {
            if (deploySignal == null) throw new ArgumentNullException("deploySignal");
            _deploySignal = deploySignal;
        }

        public override void Execute()
        {
            Log.TraceMessage();

            var activator = injectionBinder.GetInstance<TriggerActivator>();
            _deploySignal.AddListener(activator.ActivateTriggerFor);

            Log.Verbose("Created trigger activator");
        }
    }
}
