using System;
using System.Collections.Generic;
using ReeperCommon.Gui.Window;
using ReeperCommon.Gui.Window.Buttons;
using ReeperCommon.Gui.Window.Decorators;
using strange.extensions.signal.impl;
using UnityEngine;

namespace ScienceAlert.VesselContext.Gui
{
// ReSharper disable once ClassNeverInstantiated.Global
    public class ExperimentListView : StrangeView
    {
        private static readonly GUILayoutOption[] DefaultLayoutOptions = new GUILayoutOption[0]; // Used to avoid allocation of empty GUILayoutOption arrays in GUILayout calls
        private static readonly GUILayoutOption[] ExperimentButtonOptions = {GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(false)};
        
        

        [Inject(GuiKeys.CompactSkin)] public GUISkin WindowSkin { get; set; }

        [Inject(GuiKeys.WindowTitleBarButtonStyle)] public GUIStyle TitleBarButtonStyle { get; set; }
        [Inject(GuiKeys.CloseButtonTexture)] public Texture2D CloseButtonTexture { get; set; }
        [Inject(GuiKeys.LockButtonTexture)] public Texture2D LockButtonTexture { get; set; }
        [Inject(GuiKeys.UnlockButtonTexture)] public Texture2D UnlockButtonTexture { get; set; }
        [Inject(GuiKeys.ResizeCursorTexture)] public Texture2D ResizeCursorTexture { get; set; }
        [Inject(GuiKeys.ExperimentAlertToggleStyle)] public GUIStyle ExperimentAlertToggleStyle { get; set; }

        [Inject] public ExperimentListEntryFactory ExperimentListEntryFormatter { get; set; }




        internal readonly Signal Close = new Signal();
        internal readonly Signal LockToggle = new Signal();
        internal readonly Signal<ExperimentListEntry, ExperimentPopupType, Vector2> ExperimentPopup = new Signal<ExperimentListEntry, ExperimentPopupType, Vector2>();
        internal readonly Signal CloseExperimentPopup = new Signal();

        private BasicTitleBarButton _lockButton;
        private bool _popupOpenFlag = false;

        private readonly Dictionary<ScienceExperiment, ExperimentListEntry> _experimentStatuses =
            new Dictionary<ScienceExperiment, ExperimentListEntry>();

        protected override IWindowComponent Initialize()
        {
            Skin = WindowSkin;
            Draggable = true;

            var currDim = Dimensions;
            currDim.center = new Vector2(Screen.width * 0.5f, Screen.height * 0.5f);
            Dimensions = currDim;

            var scaling = new WindowScale(this, Vector2.one);

            var clamp = new ClampToScreen(scaling);

            var tb = ConfigureTitleBar(clamp);

            var rowSize = CalculateMinRowSize(); 
            var minWindowSize = rowSize + Skin.window.CalcSize(new GUIContent()) + new Vector2(0f, LockButtonTexture.height);

            var resizable = new Resizable(tb, new Vector2(0f, ResizableHotzoneSize.y), minWindowSize, ResizeCursorTexture)
            {
                Title = string.Empty
            };

            Width = minWindowSize.x;
            Height = Mathf.Max(Height, minWindowSize.y);

            return resizable;
        }


        private IWindowComponent ConfigureTitleBar(IWindowComponent decorated)
        {
            if (decorated == null) throw new ArgumentNullException("decorated");

            var withButtons = new TitleBarButtons(decorated, TitleBarButtons.ButtonAlignment.Right, TitleBarButtonOffset);

            _lockButton = new BasicTitleBarButton(TitleBarButtonStyle, LockButtonTexture, OnLockButton);

            withButtons.AddButton(_lockButton);
            withButtons.AddButton(new BasicTitleBarButton(TitleBarButtonStyle, CloseButtonTexture, OnCloseButton));

            return withButtons;
        }


        public void SetExperimentStatus(ExperimentSensorState sensorState)
        {
            _experimentStatuses[sensorState.Experiment] = ExperimentListEntryFormatter.Create(sensorState);
        }


        public override void OnUpdate()
        {
            base.OnUpdate();
            ClearPopupOpenedFlag();
        }


        protected override void DrawWindow()
        {
            GUILayout.BeginHorizontal(DefaultLayoutOptions);
            GUILayout.FlexibleSpace();
            GUILayout.Label("Experiments", DefaultLayoutOptions);
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            GUILayout.BeginScrollView(Vector2.zero, false, true, DefaultLayoutOptions);
            {
                DrawExperimentList();
            }
            GUILayout.EndScrollView();
            GUILayout.Space(8f);

            if (Event.current.type != EventType.Repaint) return;

            if (!_popupOpenFlag) CloseExperimentPopupIfOpen();
        }


        private void DrawExperimentList()
        {
            foreach (var kvp in _experimentStatuses)
                DrawExperimentStatus(kvp.Value);
        }





        private void DrawAlertToggle(
            ExperimentListEntry entry,
            bool value,
            string content, 
            GUIStyle toggleStyle,
            ExperimentPopupType popupType)
        {
            GUILayout.Toggle(value, content, toggleStyle, DefaultLayoutOptions);

            if (Event.current.type != EventType.Repaint) return;

            var toggleRect = GUILayoutUtility.GetLastRect();
            if (toggleRect.Contains(Event.current.mousePosition))
                SpawnOrUpdatePopup(entry, popupType, Event.current.mousePosition);
        }


        private void DrawExperimentStatus(ExperimentListEntry display)
        {
            if (!display.DisplayInExperimentList) return;

            
            GUILayout.BeginHorizontal(DefaultLayoutOptions);
            {
                GUI.enabled = display.DeployButtonEnabled;
                GUILayout.Button(display.ExperimentTitle, ExperimentButtonOptions);
                GUI.enabled = true;

                GUILayout.Toggle(display.CollectionAlert, string.Empty, ExperimentAlertToggleStyle, DefaultLayoutOptions);
                GUILayout.Toggle(display.TransmissionAlert, string.Empty, ExperimentAlertToggleStyle, DefaultLayoutOptions);
                DrawAlertToggle(display, display.LabAlert, string.Empty, ExperimentAlertToggleStyle, ExperimentPopupType.Lab);
            }
            GUILayout.EndHorizontal();
        }


        private void SpawnOrUpdatePopup(ExperimentListEntry entry, ExperimentPopupType popupType, Vector2 relativeMousePosition)
        {
            _popupOpenFlag = true;
            ExperimentPopup.Dispatch(entry, popupType, GUIUtility.GUIToScreenPoint(relativeMousePosition));
        }


        private void CloseExperimentPopupIfOpen()
        {
            CloseExperimentPopup.Dispatch();
        }


        private void ClearPopupOpenedFlag()
        {
            _popupOpenFlag = false;
        }

        private Vector2 CalculateMinRowSize()
        {
            string longest = "Experiment" + new string('.', 10 * 2);

            // button + toggle x4
            var button = Skin.button.CalcSize(new GUIContent(longest));
            var toggle = Skin.toggle.CalcSize(new GUIContent());

            return new Vector2(button.x + 4f * toggle.x, Mathf.Max(button.y, toggle.y));
        }


        protected override void FinalizeWindow()
        {
        }


        // Calculates maximum dimensions of a button (this will be the longest experiment title)
        private Vector2 CalculateButtonSize()
        {
            return Skin.button.CalcSize(new GUIContent {text = "This is a very long experiment name"});
        }

        private Vector2 CalculateScrollbarSize()
        {
            return Skin.verticalScrollbar.CalcSize(new GUIContent());
        }


        private void OnCloseButton()
        {
            Close.Dispatch();
        }


        private void OnLockButton()
        {
            LockToggle.Dispatch();
        }


        public void Lock(bool shouldBeDraggable)
        {
            Draggable = shouldBeDraggable;

            _lockButton.Texture = shouldBeDraggable ? UnlockButtonTexture : LockButtonTexture;
        }
    }
}
