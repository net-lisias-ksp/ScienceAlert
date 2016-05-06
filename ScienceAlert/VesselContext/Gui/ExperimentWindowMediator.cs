using ReeperCommon.Logging;
using strange.extensions.mediation.impl;
using ScienceAlert.UI.ExperimentWindow;

namespace ScienceAlert.VesselContext.Gui
{
    class ExperimentWindowMediator : Mediator
    {
        [Inject] public ExperimentWindowView View { get; set; }

        public override void OnRegister()
        {
            base.OnRegister();
            Log.Warning("ExperimentWindowMediator.OnRegister");
        }

        public override void OnRemove()
        {
            Log.Warning("ExperimentWindowMediator.OnUnregister");
            base.OnRemove();
        }
    }
}
