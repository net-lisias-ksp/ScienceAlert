using System;
using strange.extensions.mediation.impl;
using UnityEngine;

namespace ScienceAlert.VesselContext.Gui
{
    [MediatedBy(typeof(ExperimentStatusReportPopupMediator))]
// ReSharper disable once ClassNeverInstantiated.Global
    public class ExperimentStatusReportPopup : View
    {
        [Inject(GuiKeys.CompactSkin)] public GUISkin WindowSkin { get; set; }
        [Inject(GuiKeys.ClipboardTexture)] public Texture2D ClipboardTexture { get; set; }
        [Inject(GuiKeys.DishTexture)] public Texture2D DishTexture { get; set; }
        [Inject(GuiKeys.SampleTexture)] public Texture2D SampleTexture { get; set; }

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

        private ExperimentStatusReport _statusReport = default(ExperimentStatusReport);
        private ExperimentView.PopupType _popupType = ExperimentView.PopupType.None;

        private Rect _rect = new Rect(0f, 0f, 20f, 50f);
        private GUIStyle _flavorTexture = GUIStyle.none; // this is the image associated with the popup type -- just a little bit extra to make it more interesting

        protected override void Awake()
        {
            base.Awake();
            _rect = CalculateMinRect();
            _flavorTexture = new GUIStyle(WindowSkin.box);
            _flavorTexture.normal.background = _flavorTexture.onNormal.background = _flavorTexture.hover.background = _flavorTexture.onHover.background =
    DishTexture;
            _flavorTexture.fixedWidth = DishTexture.width;
            _flavorTexture.fixedHeight = DishTexture.height;
        }


        protected override void Start()
        {
            base.Start();
            RebuildDisplayData();
        }


        public void SetLocation(Vector2 center)
        {
            var newLocation = _rect;

            newLocation.center = center;

            _rect = KSPUtil.ClampRectToScreen(newLocation);
            Log.Debug("Setting location to " + center);
        }


        // calculate min window size -- note that the status report and popup type won't change while the popup is open so 
        // we need only do this at the start
        private Rect CalculateMinRect()
        {
            return new Rect(0f, 0f, 100f, 100f);
        }


        // We desperately want to avoid generating any garbage in OnGUI so all the visual stuff is cached by this method
        private void RebuildDisplayData()
        {
            enabled = _popupType != ExperimentView.PopupType.None;
        }


        private void OnGUI()
        {
            /*     Experiment Title
             * ------------
             * -          - Value: 121.5 sci
             * - flavor   - While [flying/orbiting]
             * -          - [biome]
             * ------------
             */
            GUI.skin = WindowSkin;
            GUI.depth = 0;

            GUILayout.BeginArea(_rect, WindowSkin.window);
            GUILayout.BeginHorizontal();
            {
                GUILayout.Box(DishTexture, GUILayout.ExpandWidth(false), GUILayout.ExpandHeight(false));

                GUILayout.BeginVertical();
                {
                    GUILayout.Label("Value: 123.5 sci");
                    GUILayout.Label("While Flying");
                    GUILayout.Label("Desert");
                }
                GUILayout.EndVertical();
            }
            GUILayout.EndHorizontal();
            
            //GUILayout.Label("Popup here " + _popupType + " : " + _statusReport.Experiment.experimentTitle);

            GUILayout.EndArea();
        }
    }
}
