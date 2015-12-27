using ReeperCommon.Gui.Window;
using strange.extensions.mediation.impl;
using UnityEngine;

namespace ScienceAlert.VesselContext.Gui
{
    [MediatedBy(typeof(ExperimentStatusReportPopupMediator))]
// ReSharper disable once ClassNeverInstantiated.Global
    public class ExperimentStatusReportPopup : View
    {
        public ExperimentStatusReport Status
        {
            set
            {
                _statusReport = value;
                RebuildDisplayData();
            }
        }

        public ExperimentView.PopupType PopupType {
            set
            {
                _popupType = value;
                RebuildDisplayData();
            }
        } 

        public Vector2 Location = new Vector2(Screen.width * 0.5f, Screen.height * 0.5f);
        private ExperimentStatusReport _statusReport = default(ExperimentStatusReport);
        private ExperimentView.PopupType _popupType = ExperimentView.PopupType.None;

        private Rect _rect = new Rect(0f, 0f, 200f, 200f);

        protected override void Start()
        {
            base.Start();

            RebuildDisplayData();
        }


        private void RebuildDisplayData()
        {
            enabled = _popupType != ExperimentView.PopupType.None;

            _rect.center = Location;
        }


        private void OnGUI()
        {
            GUILayout.BeginArea(_rect);

            GUILayout.Label("Popup here " + _popupType + " : " + _statusReport.Experiment.experimentTitle);

            GUILayout.EndArea();

            Log.Debug("Drawing popup");
        }
    }
}
