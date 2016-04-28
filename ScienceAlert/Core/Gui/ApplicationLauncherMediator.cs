using System.Collections;
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
            //View.SetAnimationState(ApplicationLauncherView.AnimationState.Spinning);
        }


        public override void OnRemove()
        {
            base.OnRemove();
            View.Toggle.RemoveListener(OnButtonToggle);
            //AlertPanelVisibilityChanged.RemoveListener(OnAlertPanelVisibilityChanged);
        }


        private IEnumerator LoadViewAssets()
        {
            print("Loading View assets...");
            yield return StartCoroutine(AssetBundleAssetLoader.InjectAssetsAsyncHosted(View));
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
