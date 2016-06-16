using System;
using System.Linq;
using ReeperCommon.Containers;
using ReeperCommon.Logging;
using strange.extensions.command.impl;
using ScienceAlert.VesselContext.Experiments.Triggers;
using UnityEngine;

namespace ScienceAlert.VesselContext.Experiments
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class CommandDeployExperiment : Command
    {
        [Inject] public ScienceExperiment Experiment { get; set; }
        [Inject] public SignalDeployExperimentFinished FinishedSignal { get; set; }

        public override void Execute()
        {
            Profiler.BeginSample("CommandDeployExperiment.Execute");

            Log.Debug("Deploying " + Experiment.id);

            var trigger = GetDeployTriggerFor(Experiment);

            if (!trigger.Any())
            {
                Log.Error("Could not find a trigger for " + Experiment.id);
                Cancel();
                return;
            }

            if (trigger.Value.IsBusy)
            {
                Log.Warning("Trigger is busy; aborting");
                Cancel();
                return;
            }

            Retain();
            trigger.Value.Deploy().Then(FinishedDeploying).Fail(FailedToDeploy);

            Profiler.EndSample();
        }


        private void FailedToDeploy(Exception exception)
        {
            Log.Error("Failed to deploy experiment: " + exception);
            Cancel();
            FinishedSignal.Dispatch(Experiment, false);
            Release();
        }

        private void FinishedDeploying()
        {
            Log.Verbose("Finished deploying experiment");
            FinishedSignal.Dispatch(Experiment, true);
            Release();
        }


        private Maybe<IExperimentTrigger> GetDeployTriggerFor(ScienceExperiment experiment)
        {
            var binding = injectionBinder.GetBinding<IExperimentTrigger>(experiment.id);

            if (binding == null) return Maybe<IExperimentTrigger>.None;

            return injectionBinder.GetInstance<IExperimentTrigger>(experiment.id).ToMaybe();
        }
    }
}
