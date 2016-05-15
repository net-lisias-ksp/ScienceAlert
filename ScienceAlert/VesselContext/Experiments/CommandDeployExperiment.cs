using System;
using System.Collections;
using System.Collections.Generic;
using KSP.UI;
using ReeperCommon.Extensions;
using ReeperCommon.Logging;
using ReeperCommon.Utilities;
using strange.extensions.command.impl;
using UnityEngine;

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

            Retain();
            CoroutineHoster.Instance.StartCoroutine(WaitABit());
        }


        private IEnumerator WaitABit()
        {
            yield return new WaitForSeconds(5f);

            Log.Normal("Checking hierarchy");
            //UIMasterController.Instance.gameObject.PrintComponents(new DebugLog("Master"));

            Release();
        }

        private void DeploymentFinished()
        {
            Release();
        }
    }
}
