using System;
using System.Collections.ObjectModel;
using System.Linq;
using JetBrains.Annotations;
using ReeperCommon.Containers;
using ReeperCommon.Logging;
using strange.extensions.command.impl;
using ScienceAlert.VesselContext.Experiments.Trigger;

namespace ScienceAlert.VesselContext.Experiments
{
// ReSharper disable once ClassNeverInstantiated.Global
    public class CommandDeployExperiment : Command
    {
        private readonly ScienceExperiment _experiment;
        private readonly ReadOnlyCollection<ExperimentTrigger> _triggers;
        private readonly SignalDeployExperimentFinished _finishedSignal;

        public CommandDeployExperiment(
            ScienceExperiment experiment, 
            [NotNull] ReadOnlyCollection<ExperimentTrigger> triggers,
            [NotNull] SignalDeployExperimentFinished finishedSignal)
        {
            if (experiment == null) throw new ArgumentNullException("experiment");
            if (triggers == null) throw new ArgumentNullException("triggers");
            if (finishedSignal == null) throw new ArgumentNullException("finishedSignal");

            _experiment = experiment;
            _triggers = triggers;
            _finishedSignal = finishedSignal;
        }


        public override void Execute()
        {
            Log.Debug("Deploying " + _experiment.id);

            var trigger = GetDeployTriggerFor(_experiment);

            if (!trigger.Any())
            {
                Log.Error("Could not find a trigger for " + _experiment.id);
                Cancel();
                return;
            }

            if (trigger.Value.Busy)
            {
                Log.Warning("Trigger is busy; aborting");
                Cancel();
                return;
            }

            Retain();
            trigger.Value.Deploy().Then(FinishedDeploying).Fail(FailedToDeploy);
        }


        private void FailedToDeploy(Exception exception)
        {
            Log.Error("Failed to deploy experiment: " + exception);
            Cancel();
            _finishedSignal.Dispatch(_experiment, false);
            Release();
        }

        private void FinishedDeploying()
        {
            Log.Verbose("Finished deploying experiment");
            _finishedSignal.Dispatch(_experiment, true);
            Release();
        }


        private Maybe<ExperimentTrigger> GetDeployTriggerFor(ScienceExperiment experiment)
        {
            return _triggers.FirstOrDefault(tr => tr.Experiment.id == experiment.id).ToMaybe();
        }
    }
}
