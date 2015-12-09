using ScienceAlert.Gui;
using strange.extensions.injector;
using strange.extensions.mediation.impl;

namespace ScienceAlert.Core.Gui
{
    public class ApplicationLauncherMediator : Mediator
    {
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global

        [Inject] public ApplicationLauncherView View { get; set; }
        //[Inject] public SignalAppButtonCreated AppButtonCreated { get; set; }
        //[Inject] public SignalAlertPanelViewVisibilityChanged AlertPanelVisibilityChanged { get; set; }
        //[Inject] public SignalAppButtonToggled AppButtonToggled { get; set; }

        public override void OnRegister()
        {
            base.OnRegister();
            View.ButtonCreated.AddOnce(OnButtonCreated);
            View.Toggle.AddListener(OnButtonToggle);

            //AlertPanelVisibilityChanged.AddListener(OnAlertPanelVisibilityChanged);

            View.SetAnimationState(ApplicationLauncherView.Animations.Spinning);
        }


        public override void OnRemove()
        {
            base.OnRemove();
            View.Toggle.RemoveListener(OnButtonToggle);
            //AlertPanelVisibilityChanged.RemoveListener(OnAlertPanelVisibilityChanged);
        }


        private void OnButtonCreated()
        {
            //AppButtonCreated.Dispatch();
        }


        private void OnButtonToggle(bool b)
        {
            //AppButtonToggled.Dispatch(b);
        }


        private void OnAlertPanelVisibilityChanged(bool tf)
        {
            View.SetToggleState(tf);
        }
    }
}
