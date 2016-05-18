using ReeperCommon.Containers;
using ReeperCommon.Logging;
using strange.extensions.mediation.impl;
using ScienceAlert.UI.OptionsWindow;

namespace ScienceAlert.VesselContext.Gui
{
    class OptionsWindowMediator : Mediator
    {
        [Inject] public OptionsWindowView View { get; set; }
        [Inject] public SignalContextDestruction ContextDestroyed { get; set; }

        public override void OnRegister()
        {
            base.OnRegister();

            ContextDestroyed.AddOnce(OnContextDestroyed);
        }

        public override void OnRemove()
        {
            base.OnRemove();
        }


        private void OnContextDestroyed()
        {
            Log.Verbose(GetType().Name + " destroying view");
            View.Do(Destroy);
        }
    }
}
