using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using ReeperCommon.Gui.Window;
using ReeperCommon.Gui.Window.Buttons;
using ReeperCommon.Gui.Window.Decorators;
using ReeperCommon.Serialization;
using strange.extensions.implicitBind;
using strange.extensions.injector;
using strange.extensions.signal.impl;
using UnityEngine;

namespace ScienceAlert.Gui
{
    [MediatedBy(typeof(VesselDebugMediator))]
// ReSharper disable once ClassNeverInstantiated.Global
    public class VesselDebugView : StrangeView
    {
        [Inject] public GUISkin WindowSkin { get; set; }

        [Inject(Keys.WindowTitleBarButtonStyle)] public GUIStyle TitleBarButtonStyle { get; set; }
        [Inject(Keys.CloseButtonTexture)] public Texture2D CloseButtonTexture { get; set; }
        [Inject(Keys.ResizeCursorTexture)] public Texture2D ResizeCursorTexture { get; set; }

        internal readonly Signal Close = new Signal();
        internal readonly Signal RefreshScienceData = new Signal();

        [ReeperPersistent] private Vector2 _scrollPos = Vector2.zero;

        private string _vesselCoordinates = "<coords>";
        private string _vesselBiome = "<biome>";
        private string _transmitterCount = "-1";
        private string _storageCount = "-1";
        private List<string> _subjects = new List<string>();


        [PostConstruct]
        private void SetupTest()
        {
            print("VesselDebugView.SetupTest");
        }


        protected override IWindowComponent Initialize()
        {
            Skin = WindowSkin;
            Draggable = true;
            Height = 1f;
            Title = "Vessel Debug";

            Width = 200f;
            Height = 300f;


            var scaling = new WindowScale(this, Vector2.one);
            var clamp = new ClampToScreen(scaling);
            var tb = new TitleBarButtons(clamp);

            tb.AddButton(new BasicTitleBarButton(TitleBarButtonStyle, CloseButtonTexture, OnCloseButton));

            var resizable = new Resizable(tb, new Vector2(0f, ResizableHotzoneSize.y), MinWindowSize, ResizeCursorTexture);


            var currDim = Dimensions;
            currDim.center = new Vector2(Screen.width * 0.5f, Screen.height * 0.5f);
            Dimensions = currDim;


            return resizable;
        }


        protected override void DrawWindow()
        {
            GUILayout.BeginVertical();
            {
                DrawSpacedHorizontalSection("Position:", _vesselCoordinates);
                DrawSpacedHorizontalSection("Biome:", _vesselBiome);
                GUILayout.Space(20f);
                DrawSpacedHorizontalSection("Transmitters:", _transmitterCount);
                DrawSpacedHorizontalSection("Storage Containers:", _storageCount);

                GUILayout.Space(20f);
                GUILayout.BeginHorizontal();
                {
                    GUILayout.Label("Stored SubjectIDs");
                    GUILayout.FlexibleSpace();
                    if (GUILayout.Button("Refresh", GUILayout.ExpandWidth(false), GUILayout.ExpandHeight(false)))
                        RefreshScienceData.Dispatch();
                }
                GUILayout.EndHorizontal();
                _scrollPos = GUILayout.BeginScrollView(_scrollPos);
                {
// ReSharper disable once ForCanBeConvertedToForeach
                    for (int i = 0; i < _subjects.Count; ++i)
                        GUILayout.Label(_subjects[i]);
                }
                GUILayout.EndScrollView();
            }
            GUILayout.EndVertical();
        }


        private static void DrawSpacedHorizontalSection(string label, string value)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label(label);
            GUILayout.FlexibleSpace();
            GUILayout.Label(value);
            GUILayout.EndHorizontal();
        }


        protected override void FinalizeWindow()
        {

        }


        private void OnCloseButton()
        {
            Close.Dispatch();
        }


        public void SetVesselCoordinates(double lat, double lon)
        {
            _vesselCoordinates = string.Format("{0}.2,{1}.2", FlightGlobals.ActiveVessel.latitude,
                FlightGlobals.ActiveVessel.longitude);
        }


        public void SetVesselBiome(string biome)
        {
            _vesselBiome = biome;
        }

        public void SetVesselTransmitterCount(int count)
        {
            _transmitterCount = count.ToString(CultureInfo.InvariantCulture);
        }


        public void SetVesselStorageContainerCount(int count)
        {
            _storageCount = count.ToString(CultureInfo.InvariantCulture);
        }


        public void SetVesselContainerSubjects(IEnumerable<string> subjectIds)
        {
            _subjects = (subjectIds ?? Enumerable.Empty<string>()).ToList();
        }
    }
}
