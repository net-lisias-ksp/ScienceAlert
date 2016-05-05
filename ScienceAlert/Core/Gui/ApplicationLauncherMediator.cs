using System.Collections;
using System.Linq;
using ReeperCommon.Logging;
using ReeperKSP.AssetBundleLoading;
using strange.extensions.mediation.impl;

namespace ScienceAlert.Core.Gui
{
    public class ApplicationLauncherMediator : Mediator
    {
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global

        [Inject] public ApplicationLauncherView View { get; set; }

        public override void OnRegister()
        {
            base.OnRegister();
            View.ButtonCreated.AddOnce(OnButtonCreated);
            View.Toggle.AddListener(OnButtonToggle);

            View.enabled = false; // prevent View from starting up while we wait on assets

            StartCoroutine(LoadViewAssets());
        }


        public override void OnRemove()
        {
            base.OnRemove();
            View.Toggle.RemoveListener(OnButtonToggle);
        }


        private IEnumerator LoadViewAssets()
        {
            print("Loading View assets...");

            var viewAssetLoadRoutine = AssetBundleAssetLoader.InjectAssetsAsync(View, View.GetType());
            yield return viewAssetLoadRoutine.YieldUntilComplete;

            if (viewAssetLoadRoutine.Error.Any())
            {
                Log.Error("Failed to load ApplicationLauncher assets: " + viewAssetLoadRoutine.Error.Value);
                yield break;
            }

            print("View assets loaded, waiting for AppLauncher");


            yield return StartCoroutine(View.SetupButton());

            print("Finished waiting on AppLauncher, enabling View");

            View.enabled = true;
        }


        private void OnButtonCreated()
        {
            //AppButtonCreated.Dispatch();
        }


        private void OnButtonToggle(bool b)
        {
            //AppButtonToggled.Dispatch(b);
        }


        //private void OnAlertPanelVisibilityChanged(bool tf)
        //{
        //    View.SetToggleState(tf);
        //}
    }
}
