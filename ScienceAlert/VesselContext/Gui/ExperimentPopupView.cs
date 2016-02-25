using System;
using System.Linq;
using ReeperCommon.Logging;
using strange.extensions.mediation.impl;
using UnityEngine;

namespace ScienceAlert.VesselContext.Gui
{
    public enum ExperimentPopupType
    {
        Default,
        Collection,
        Transmission,
        Lab
    }

    [MediatedBy(typeof(ExperimentPopupMediator))]
// ReSharper disable once ClassNeverInstantiated.Global
    public class ExperimentPopupView : View
    {
        private static readonly GUILayoutOption[] DefaultLayoutOptions = new GUILayoutOption[0];
        private static readonly GUILayoutOption[] FlavorTextureOptions = {GUILayout.ExpandWidth(false), GUILayout.ExpandHeight(false)};

        [Inject(GuiKeys.PopupSkin)] public GUISkin WindowSkin { get; set; }
        [Inject(GuiKeys.ClipboardTexture)] public Texture2D ClipboardTexture { get; set; }
        [Inject(GuiKeys.DishTexture)] public Texture2D DishTexture { get; set; }
        [Inject(GuiKeys.SampleTexture)] public Texture2D SampleTexture { get; set; }


        public Rect WindowRect = new Rect(0f, 0f, 20f, 50f);
        private GUIStyle _flavorTexture = GUIStyle.none; // this is the image associated with the popup type -- just a little bit extra to make it more interesting
        private GUIStyle _labelStyle = GUIStyle.none;

        private string _experiment = string.Empty;
        private string[] _displayStrings = new string[0];
        
        protected override void Awake()
        {
            base.Awake();
            _flavorTexture = new GUIStyle(WindowSkin.box);

            _flavorTexture.fixedWidth = DishTexture.width;
            _flavorTexture.fixedHeight = DishTexture.height;

            _labelStyle = new GUIStyle(WindowSkin.label);
            _labelStyle.wordWrap = false;

        }


        protected override void Start()
        {
            base.Start();
            enabled = false;
        }


        public void RebuildDisplayData(ExperimentListEntry sourceData, ExperimentPopupType popupType)
        {
            print("RebuildDisplayData");

            _experiment = sourceData.ExperimentTitle;

            var popupFlavorTexture = _flavorTexture.normal.background;

            switch (popupType)
            {
                case ExperimentPopupType.Collection:
                    _displayStrings = new[]
                    {
                        "Collection Value: " + sourceData.CollectionValue.ToString("F2")
                    };

                    popupFlavorTexture = SampleTexture;

                    break;

                case ExperimentPopupType.Transmission:
                    _displayStrings = new[]
                    {
                        "Collection Value: " + sourceData.CollectionValue.ToString("F2")
                    };

                    popupFlavorTexture = DishTexture;

                    break;

                case ExperimentPopupType.Lab:
                    _displayStrings = new[]
                    {
                        "Collection Value: " + sourceData.CollectionValue.ToString("F2")
                    };

                    popupFlavorTexture = ClipboardTexture;

                    break;

                case ExperimentPopupType.Default:
                    _displayStrings = Enumerable.Empty<string>().ToArray();
                    break;

                default:
                    Log.Error("Unknown popup type: " + popupType);
                    break;
            }

            _flavorTexture.normal.background = 
                _flavorTexture.onNormal.background = 
                _flavorTexture.hover.background =
                _flavorTexture.onHover.background =
                    popupFlavorTexture;

            var newRect = CalculateWindowRect();

            newRect.center = WindowRect.center;

            WindowRect = newRect;

            Log.Warning("Fix popup size calculation; sometimes won't highlight when mousing over from scrollbar");
        }


        private Rect CalculateWindowRect()
        {
            var minSize = default(Rect);

            minSize.width = _flavorTexture.fixedWidth + _flavorTexture.contentOffset.x + _flavorTexture.padding.left +
                            _flavorTexture.padding.right +
                            _displayStrings.Max<string, float>(
                                displayString =>
                                    _labelStyle.contentOffset.x + _labelStyle.padding.left + _labelStyle.padding.right +
                                    _labelStyle.CalcSize(new GUIContent(displayString)).x) + 100f;
            minSize.height = 200f;

            return minSize;
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

            GUILayout.Label(_experiment);
            GUILayout.BeginHorizontal(DefaultLayoutOptions);
            {
                GUILayout.Box(DishTexture, FlavorTextureOptions);

                GUILayout.BeginVertical(DefaultLayoutOptions);
                {
                    foreach (var s in _displayStrings)
                        GUILayout.Label(s, GUILayout.MinWidth(120f), GUILayout.ExpandHeight(false));
                }
                GUILayout.EndVertical();
            }
            GUILayout.EndHorizontal();
            
            GUILayout.EndArea();
        }
    }
}
