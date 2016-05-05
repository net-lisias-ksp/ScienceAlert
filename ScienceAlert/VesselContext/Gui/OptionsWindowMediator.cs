using ReeperCommon.Logging;
using strange.extensions.mediation.impl;
using ScienceAlert.UI.OptionsWindow;

namespace ScienceAlert.VesselContext.Gui
{
    [Mediates(typeof(OptionsWindowView))]
    class OptionsWindowMediator : Mediator
    {
        [Inject] public OptionsWindowView View { get; set; }

        public override void OnRegister()
        {
            base.OnRegister();
            Log.Warning("OptionsWindowMediator.OnRegister");
        }

        public override void OnRemove()
        {
            base.OnRemove();
        }
    }
}
