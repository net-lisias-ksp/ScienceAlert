using ReeperCommon.Containers;
using ReeperCommon.Logging;
using strange.extensions.mediation.impl;
using ScienceAlert.UI.OptionsWindow;

namespace ScienceAlert.VesselContext.Gui
{
    class OptionsWindowMediator : Mediator
    {
        [Inject] public OptionsWindowView View { get; set; }
        [Inject] public SignalContextIsBeingDestroyed ContextDestroyed { get; set; }

        public override void OnRegister()
        {
            base.OnRegister();

            // View signals
            View.CloseButtonClicked.AddListener(OnCloseButtonClicked);

            // other signals
            ContextDestroyed.AddOnce(OnContextDestroyed);
        }


        public override void OnRemove()
        {
            View.CloseButtonClicked.RemoveListener(OnCloseButtonClicked);

            base.OnRemove();
        }


        private void OnCloseButtonClicked()
        {
            View.gameObject.SetActive(false);
        }


        private void OnContextDestroyed()
        {
            Log.Verbose(GetType().Name + " destroying view");
            Destroy(gameObject);
        }
    }
}
