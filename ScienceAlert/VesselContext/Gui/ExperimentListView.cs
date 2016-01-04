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
        [Inject(GuiKeys.CompactSkin)] public GUISkin WindowSkin { get; set; }

        [Inject(GuiKeys.WindowTitleBarButtonStyle)] public GUIStyle TitleBarButtonStyle { get; set; }
        [Inject(GuiKeys.CloseButtonTexture)] public Texture2D CloseButtonTexture { get; set; }
        [Inject(GuiKeys.LockButtonTexture)] public Texture2D LockButtonTexture { get; set; }
        [Inject(GuiKeys.UnlockButtonTexture)] public Texture2D UnlockButtonTexture { get; set; }
        [Inject(GuiKeys.ResizeCursorTexture)] public Texture2D ResizeCursorTexture { get; set; }
        [Inject(GuiKeys.LitToggleStyle)] public GUIStyle LitToggleStyle { get; set; }

        public enum PopupType
        {
            None,
            Alert,
            Collection,
            Transmission,
            Lab
        }

        class ExperimentReportDisplay
        {
            public readonly ExperimentStatusReport Report;

            public readonly GUIContent AlertContent;
            public readonly GUIContent CollectionContent;
            public readonly GUIContent TransmissionContent;
            public readonly GUIContent LabContent;

            private ExperimentReportDisplay(
                ExperimentStatusReport report, 
                GUIContent alertContent, 
                GUIContent collectionContent,
                GUIContent transmissionContent, 
                GUIContent labContent)
            {
                if (alertContent == null) throw new ArgumentNullException("AlertContent");
                if (collectionContent == null) throw new ArgumentNullException("CollectionContent");
                if (transmissionContent == null) throw new ArgumentNullException("TransmissionContent");
                if (labContent == null) throw new ArgumentNullException("LabContent");
                Report = report;
                AlertContent = alertContent;
                CollectionContent = collectionContent;
                TransmissionContent = transmissionContent;
                LabContent = labContent;
            }


            public class Factory
            {
                public ExperimentReportDisplay Create(ExperimentStatusReport report)
                {
                    var exp = report.Experiment;

                    return new ExperimentReportDisplay(report, CreateContent(exp, PopupType.Alert),
                        CreateContent(exp, PopupType.Collection), CreateContent(exp, PopupType.Transmission),
                        CreateContent(exp, PopupType.Lab));
                }


                private static GUIContent CreateContent(ScienceExperiment experiment, PopupType popup)
                {
                    return new GUIContent(string.Empty, popup.ToString());
                }
            }
        }

        internal readonly Signal Close = new Signal();
        internal readonly Signal LockToggle = new Signal();
        internal readonly Signal<ExperimentStatusReport, PopupType, Vector2> ExperimentPopup = new Signal<ExperimentStatusReport, PopupType, Vector2>();

        private BasicTitleBarButton _lockButton;
        private readonly ExperimentReportDisplay.Factory _displayFactory = new ExperimentReportDisplay.Factory();

        private readonly Dictionary<ScienceExperiment, ExperimentReportDisplay> _experimentStatuses =
            new Dictionary<ScienceExperiment, ExperimentReportDisplay>(); 


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


        public void SetExperimentStatus(ExperimentStatusReport statusReport)
        {
            _experimentStatuses[statusReport.Experiment] = _displayFactory.Create(statusReport);
        }


        protected override void DrawWindow()
        {
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            GUILayout.Label("Experiments");
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            GUILayout.BeginScrollView(Vector2.zero, false, true);
            {
                DrawExperimentList();
            }
            GUILayout.EndScrollView();
            GUILayout.Space(8f);
        }


        private static bool ShouldDisplayExperimentInList(ExperimentStatusReport statusReport)
        {
            return statusReport.Onboard;
        }


        private void DrawExperimentList()
        {
            var experimentEnumerator = _experimentStatuses.GetEnumerator(); // avoid Using... because Mono boxes iterator creating extra garbage

            try
            {
                ExperimentReportDisplay mousedOverReport = null;

                while (experimentEnumerator.MoveNext())
                {
                    var item = experimentEnumerator.Current.Value;
                    DrawExperimentStatus(item);

                    if (!string.IsNullOrEmpty(GUI.tooltip)) mousedOverReport = item;
                }

                if (Event.current.type != EventType.Repaint) return;

                UpdateExperimentPopup(mousedOverReport, Event.current.mousePosition);
            }
            finally
            {
                experimentEnumerator.Dispose();
            }
        }


        private void UpdateExperimentPopup(ExperimentReportDisplay display, Vector2 popupLocation)
        {
            try
            {
                if (!string.IsNullOrEmpty(GUI.tooltip) && display != null)
                {
                    var screenMousePos = GUIUtility.GUIToScreenPoint(popupLocation);
                    if (Dimensions.Contains(screenMousePos))
                    {
                        var popupType = (PopupType) Enum.Parse(typeof (PopupType), GUI.tooltip);

                        ExperimentPopup.Dispatch(display.Report, popupType, GUIUtility.GUIToScreenPoint(popupLocation));
                        return;
                    }
                }
                
                ExperimentPopup.Dispatch(default(ExperimentStatusReport), PopupType.None, popupLocation);
            }
            catch (Exception e)
            {
                if (!(e is ArgumentNullException) && !(e is ArgumentException) && !(e is OverflowException)) throw;

                Log.Error("Unrecognized display popup name: " + GUI.tooltip);
                return;
            }
        }


        private void DrawExperimentStatus(ExperimentReportDisplay display)
        {
            if (!ShouldDisplayExperimentInList(display.Report)) return;

            GUILayout.BeginHorizontal();
            {
                GUILayout.Button(display.Report.Experiment.experimentTitle, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(false));

                var report = display.Report;

                GUILayout.Toggle(true, display.CollectionContent, LitToggleStyle);
                GUILayout.Toggle(report.CollectionValue > 0f, display.CollectionContent, LitToggleStyle);
                GUILayout.Toggle(report.TransmissionValue > 0f, display.TransmissionContent, LitToggleStyle);
                GUILayout.Toggle(report.LabValue > 0f, display.LabContent, LitToggleStyle);

            }
            GUILayout.EndHorizontal();
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



        public override void OnUpdate()
        {
            base.OnUpdate();
            return;

            //if (_hasSetInitialSizeFlag) return;

            //_hasSetInitialSizeFlag = true;

            //_minimumWidth = Mathf.Max(AbsoluteMinimumButtonWidth, CalculateButtonSize().x);

            //var sbWidth = CalculateScrollbarSize().x;

            //WindowOptions = new []
            //{
            //    GUILayout.MinWidth(
            //    _minimumWidth + 
            //        Skin.window.padding.left + 
            //        Skin.window.padding.right + 
            //        Skin.window.margin.left + 
            //        Skin.window.margin.right +
                    
            //        Skin.scrollView.padding.left +
            //        Skin.scrollView.padding.right +
            //        Skin.scrollView.margin.left +
            //        Skin.scrollView.margin.right +

            //        sbWidth //+
            //        //Skin.horizontalScrollbar.padding.left +
            //        //Skin.horizontalScrollbar.padding.right + 
            //        //Skin.horizontalScrollbar.margin.left +
            //        //Skin.horizontalScrollbar.margin.right
            //        ),
            //    GUILayout.MinHeight(160f)
            //};

            //print("MinWidth: " + _minimumWidth);
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
