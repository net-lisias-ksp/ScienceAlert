using strange.extensions.mediation.impl;
using UnityEngine;

namespace ScienceAlert.VesselContext.Gui
{
    public class ExperimentStatusReportPopupMediator : Mediator
    {
        [Inject] public ExperimentStatusReportPopup View { get; set; }

        [Inject] public SignalUpdateExperimentReportPopupLocation UpdatePositionSignal { get; set; }

        public override void OnRegister()
        {
            base.OnRegister();
            Log.Warning("Registering position change signal");
            UpdatePositionSignal.AddListener(OnPopupLocationChanged);
        }


        public override void OnRemove()
        {
            Log.Warning("Unregistering position change signal");
            UpdatePositionSignal.RemoveListener(OnPopupLocationChanged);
            base.OnRemove();
        }


        public void OnPopupLocationChanged(Vector2 mouseLocation)
        {
            Log.Verbose("Received OnPopupLocationChanged: " + mouseLocation);
            View.SetLocation(mouseLocation);
        }
    }
}
