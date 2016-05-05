//using System;
//using System.Collections.Generic;
//using ReeperKSP.Gui.Window;
//using ReeperKSP.Gui.Window.Buttons;
//using ReeperKSP.Gui.Window.Decorators;
//using strange.extensions.signal.impl;
//using UnityEngine;

//namespace ScienceAlert.VesselContext.Gui
//{
//// ReSharper disable once ClassNeverInstantiated.Global
//    public class ExperimentListView : StrangeView
//    {
//        private readonly GUILayoutOption[] _defaultLayoutOptions = new GUILayoutOption[0]; // Used to avoid allocation of empty GUILayoutOption arrays in GUILayout calls
//        private readonly GUILayoutOption[] _experimentButtonOptions = {GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(false)};
        
        

//        [Inject(GuiKeys.CompactSkin)] public GUISkin WindowSkin { get; set; }

//        [Inject(GuiKeys.WindowTitleBarButtonStyle)] public GUIStyle TitleBarButtonStyle { get; set; }
//        [Inject(GuiKeys.CloseButtonTexture)] public Texture2D CloseButtonTexture { get; set; }
//        [Inject(GuiKeys.LockButtonTexture)] public Texture2D LockButtonTexture { get; set; }
//        [Inject(GuiKeys.UnlockButtonTexture)] public Texture2D UnlockButtonTexture { get; set; }
//        [Inject(GuiKeys.ResizeCursorTexture)] public Texture2D ResizeCursorTexture { get; set; }
//        [Inject(GuiKeys.ExperimentAlertToggleStyle)] public GUIStyle ExperimentAlertToggleStyle { get; set; }

//        [Inject] public ExperimentListEntryFactory ExperimentListEntryFormatter { get; set; }




//        internal readonly Signal Close = new Signal();
//        internal readonly Signal LockToggle = new Signal();
//        internal readonly Signal<ExperimentListEntry, ExperimentPopupType, Vector2> ExperimentPopup = new Signal<ExperimentListEntry, ExperimentPopupType, Vector2>();
//        internal readonly Signal CloseExperimentPopup = new Signal();
//        internal readonly Signal<ScienceExperiment> DeployExperiment = new Signal<ScienceExperiment>();

//        private BasicTitleBarButton _lockButton;
//        private bool _popupOpenFlag = false;

//        private readonly Dictionary<ScienceExperiment, ExperimentListEntry> _experimentStatuses =
//            new Dictionary<ScienceExperiment, ExperimentListEntry>();

//        protected override IWindowComponent Initialize()
//        {
//            Skin = WindowSkin;
//            Draggable = true;

//            var currDim = Dimensions;
//            currDim.center = new Vector2(Screen.width * 0.5f, Screen.height * 0.5f);
//            Dimensions = currDim;

//            var scaling = new WindowScale(this, Vector2.one);

//            var clamp = new ClampToScreen(scaling);

//            var tb = ConfigureTitleBar(clamp);

//            var rowSize = CalculateMinRowSize(); 
//            var minWindowSize = rowSize + Skin.window.CalcSize(new GUIContent()) + new Vector2(0f, LockButtonTexture.height);

//            var resizable = new Resizable(tb, new Vector2(0f, ResizableHotzoneSize.y), minWindowSize, ResizeCursorTexture)
//            {
//                Title = string.Empty
//            };

//            Width = minWindowSize.x;
//            Height = Mathf.Max(Height, minWindowSize.y);

//            return resizable;
//        }


//        private IWindowComponent ConfigureTitleBar(IWindowComponent decorated)
//        {
//            if (decorated == null) throw new ArgumentNullException("decorated");

//            var withButtons = new TitleBarButtons(decorated, TitleBarButtons.ButtonAlignment.Right, TitleBarButtonOffset);

//            _lockButton = new BasicTitleBarButton(TitleBarButtonStyle, LockButtonTexture, OnLockButton);

//            withButtons.AddButton(_lockButton);
//            withButtons.AddButton(new BasicTitleBarButton(TitleBarButtonStyle, CloseButtonTexture, OnCloseButton));

//            return withButtons;
//        }


//        public void SetExperimentStatus(ExperimentSensorState sensorState)
//        {
//            _experimentStatuses[sensorState.Experiment] = ExperimentListEntryFormatter.Create(sensorState);
//        }


//        public override void OnUpdate()
//        {
//            base.OnUpdate();
//            ClearPopupOpenedFlag();
//        }


//        protected override void DrawWindow()
//        {
//            GUILayout.BeginHorizontal(_defaultLayoutOptions);
//            GUILayout.FlexibleSpace();
//            GUILayout.Label("Experiments", _defaultLayoutOptions);
//            GUILayout.FlexibleSpace();
//            GUILayout.EndHorizontal();

//            GUILayout.BeginScrollView(Vector2.zero, false, true, _defaultLayoutOptions);
//            {
//                DrawExperimentList();
//            }
//            GUILayout.EndScrollView();
//            GUILayout.Space(8f);

//            if (Event.current.type != EventType.Repaint) return;

//            if (!_popupOpenFlag) CloseExperimentPopupIfOpen();
//        }


//        private void DrawExperimentList()
//        {
//            foreach (var kvp in _experimentStatuses)
//                DrawExperimentStatus(kvp.Value);
//        }





//        private void DrawAlertToggle(
//            ExperimentListEntry entry,
//            bool value,
//            string content, 
//            GUIStyle toggleStyle,
//            ExperimentPopupType popupType)
//        {
//            GUILayout.Toggle(value, content, toggleStyle, _defaultLayoutOptions);

//            if (Event.current.type != EventType.Repaint) return;

//            var toggleRect = GUILayoutUtility.GetLastRect();
//            if (toggleRect.Contains(Event.current.mousePosition))
//                SpawnOrUpdatePopup(entry, popupType, Event.current.mousePosition);
//        }


//        private void DrawExperimentStatus(ExperimentListEntry display)
//        {
//            if (!display.DisplayInExperimentList) return;

            
//            GUILayout.BeginHorizontal(_defaultLayoutOptions);
//            {
//                GUI.enabled = display.DeployButtonEnabled;
//                if (GUILayout.Button(display.ExperimentTitle, _experimentButtonOptions))
//                    DeployExperiment.Dispatch(display.Experiment);
//                GUI.enabled = true;

//                GUILayout.Toggle(display.CollectionAlert, string.Empty, ExperimentAlertToggleStyle, _defaultLayoutOptions);
//                GUILayout.Toggle(display.TransmissionAlert, string.Empty, ExperimentAlertToggleStyle, _defaultLayoutOptions);
//                DrawAlertToggle(display, display.LabAlert, string.Empty, ExperimentAlertToggleStyle, ExperimentPopupType.Lab);
//            }
//            GUILayout.EndHorizontal();
//        }



//        private void SpawnOrUpdatePopup(ExperimentListEntry entry, ExperimentPopupType popupType, Vector2 relativeMousePosition)
//        {
//            _popupOpenFlag = true;
//            ExperimentPopup.Dispatch(entry, popupType, GUIUtility.GUIToScreenPoint(relativeMousePosition));
//        }


//        private void CloseExperimentPopupIfOpen()
//        {
//            CloseExperimentPopup.Dispatch();
//        }


//        private void ClearPopupOpenedFlag()
//        {
//            _popupOpenFlag = false;
//        }

//        private Vector2 CalculateMinRowSize()
//        {
//            string longest = "Experiment" + new string('.', 10 * 2);

//            // button + toggle x4
//            var button = Skin.button.CalcSize(new GUIContent(longest));
//            var toggle = Skin.toggle.CalcSize(new GUIContent());

//            return new Vector2(button.x + 4f * toggle.x, Mathf.Max(button.y, toggle.y));
//        }


//        protected override void FinalizeWindow()
//        {
//        }


//        // Calculates maximum dimensions of a button (this will be the longest experiment title)
//        private Vector2 CalculateButtonSize()
//        {
//            return Skin.button.CalcSize(new GUIContent {text = "This is a very long experiment name"});
//        }

//        private Vector2 CalculateScrollbarSize()
//        {
//            return Skin.verticalScrollbar.CalcSize(new GUIContent());
//        }


//        private void OnCloseButton()
//        {
//            Close.Dispatch();
//        }


//        private void OnLockButton()
//        {
//            LockToggle.Dispatch();
//        }


//        public void Lock(bool shouldBeDraggable)
//        {
//            Draggable = shouldBeDraggable;

//            _lockButton.Texture = shouldBeDraggable ? UnlockButtonTexture : LockButtonTexture;
//        }
//    }
//}
