using System;
using ReeperCommon.Logging;
using strange.extensions.command.impl;

namespace ScienceAlert.VesselContext.Experiments
{
// ReSharper disable once ClassNeverInstantiated.Global
    class CommandDeployExperiment : Command
    {
        private readonly ScienceExperiment _experiment;

        public CommandDeployExperiment(
            ScienceExperiment experiment)
        {
            if (experiment == null) throw new ArgumentNullException("experiment");

            _experiment = experiment;
        }

        public override void Execute()
        {
            Log.Debug("Deploying " + _experiment.id);

            //var deployer = _deploymentStrategySelector.GetDeploymentStrategy(_experiment);

            //Retain();

            //try
            //{
            //    deployer.Deploy().Then(DeploymentFinished);
            //}
            //catch (Exception e)
            //{
            //    Log.Error("Failed to deploy " + _experiment.id + "!");
            //    Log.Error("Exception: " + e);
            //    Cancel();
            //    Release();
            //}
        }


        private void DeploymentFinished()
        {
            Release();
        }
    }
}
