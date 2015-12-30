using System;
using strange.extensions.mediation.impl;
using UnityEngine;

namespace ScienceAlert.VesselContext.Gui
{
    [MediatedBy(typeof(ExperimentPopupMediator))]
// ReSharper disable once ClassNeverInstantiated.Global
    public class ExperimentPopupView : View
    {
        [Inject(GuiKeys.PopupSkin)] public GUISkin WindowSkin { get; set; }
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
            get { return _statusReport; }
        }

        public ExperimentListView.PopupType PopupType {
            set
            {
                _popupType = value;
                RebuildDisplayData();
            }
            get { return _popupType; }
        } 

        private ExperimentStatusReport _statusReport = default(ExperimentStatusReport);
        private ExperimentListView.PopupType _popupType = ExperimentListView.PopupType.None;

        public Rect WindowRect = new Rect(0f, 0f, 20f, 50f);
        private GUIStyle _flavorTexture = GUIStyle.none; // this is the image associated with the popup type -- just a little bit extra to make it more interesting

        protected override void Awake()
        {
            base.Awake();
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
            var newLocation = WindowRect;

            newLocation.center = center;

            WindowRect = KSPUtil.ClampRectToScreen(newLocation);
        }



        // We desperately want to avoid generating any garbage in OnGUI so all the visual stuff is cached by this method
        private void RebuildDisplayData()
        {
            enabled = _popupType != ExperimentListView.PopupType.None;
        }


// ReSharper disable once UnusedMember.Local
// ReSharper disable once InconsistentNaming
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

            GUILayout.BeginArea(WindowRect, WindowSkin.window);
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
