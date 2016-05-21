using System;
using System.Collections;
using System.Linq;
using ReeperCommon.Logging;
using ReeperKSP.AssetBundleLoading;
using strange.extensions.mediation.impl;

namespace ScienceAlert.Core.Gui
{
    class ApplicationLauncherMediator : Mediator
    {
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global

        [Inject] public ApplicationLauncherView View { get; set; }

        [Inject] public SignalScienceAlertIssued AlertIssueSignal { get; set; }

        public override void OnRegister()
        {
            base.OnRegister();
            View.ButtonCreated.AddOnce(OnButtonCreated);
            View.Toggle.AddListener(OnButtonToggle);

            View.enabled = false; // prevent View from starting up while we wait on assets

            StartCoroutine(Initialize());
        }


        public override void OnRemove()
        {
            base.OnRemove();
            View.Toggle.RemoveListener(OnButtonToggle);
            AlertIssueSignal.RemoveListener(OnAlertIssued);
        }


        private IEnumerator Initialize()
        {
            yield return StartCoroutine(LoadViewAssets());

            AlertIssueSignal.AddListener(OnAlertIssued);
        }


        private IEnumerator LoadViewAssets()
        {
            Log.Verbose("Loading View assets...");

            var viewAssetLoadRoutine = AssetBundleAssetLoader.InjectAssetsAsync(View, View.GetType());
            yield return viewAssetLoadRoutine.YieldUntilComplete;

            if (viewAssetLoadRoutine.Error.Any())
            {
                Log.Error("Failed to load ApplicationLauncher assets: " + viewAssetLoadRoutine.Error.Value);
                yield break;
            }

            Log.Verbose("View assets loaded, waiting for AppLauncher");


            yield return StartCoroutine(View.SetupButton());

            Log.Verbose("Finished waiting on AppLauncher, enabling View");

            View.enabled = true;
        }


        private void OnButtonCreated()
        {
            Log.Error(GetType().Name + " button created");
            //AppButtonCreated.Dispatch();
        }


        private void OnButtonToggle(bool b)
        {
            View.SetToggleState(false);
            
            if (View.AnimationState == ApplicationLauncherView.ButtonAnimationStates.Spinning)
                View.AnimationState = ApplicationLauncherView.ButtonAnimationStates.Lit;
          
        }


        private void OnAlertIssued()
        {
            View.AnimationState = ApplicationLauncherView.ButtonAnimationStates.Spinning;
        }
    }
}
