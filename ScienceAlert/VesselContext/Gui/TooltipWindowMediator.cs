using ReeperCommon.Logging;
using strange.extensions.mediation.impl;
using ScienceAlert.UI.TooltipWindow;

namespace ScienceAlert.VesselContext.Gui
{
    class TooltipWindowMediator : Mediator
    {
        [Inject] public TooltipWindowView View { get; set; }

        public override void OnRegister()
        {
            base.OnRegister();
            View.SetTooltip("Hello, world!");
            Log.Normal("tooltip was definitely set");
            //View.gameObject.SetActive(false);
        }

        public override void OnRemove()
        {
            base.OnRemove();
        }
    }
}
