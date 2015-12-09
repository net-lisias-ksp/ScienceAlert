using System;
using ReeperCommon.Gui.Window;
using ReeperCommon.Gui.Window.Buttons;
using ReeperCommon.Gui.Window.Decorators;
using ReeperCommon.Serialization.Exceptions;
using strange.extensions.implicitBind;
using strange.extensions.injector;
using strange.extensions.signal.impl;
using UnityEngine;

namespace ScienceAlert.VesselContext.Gui
{
// ReSharper disable once ClassNeverInstantiated.Global
    [MediatedBy(typeof(ExperimentMediator))]
    public class ExperimentView : StrangeView
    {
        [Inject(GuiKeys.CompactSkin)] public GUISkin WindowSkin { get; set; }

        [Inject(GuiKeys.WindowTitleBarButtonStyle)] public GUIStyle TitleBarButtonStyle { get; set; }
        [Inject(GuiKeys.CloseButtonTexture)] public Texture2D CloseButtonTexture { get; set; }
        [Inject(GuiKeys.LockButtonTexture)] public Texture2D LockButtonTexture { get; set; }
        [Inject(GuiKeys.UnlockButtonTexture)] public Texture2D UnlockButtonTexture { get; set; }
        [Inject(GuiKeys.ResizeCursorTexture)] public Texture2D ResizeCursorTexture { get; set; }
        [Inject(GuiKeys.LitToggleStyle)] public GUIStyle LitToggleStyle { get; set; }


        internal readonly Signal Close = new Signal();
        internal readonly Signal LockToggle = new Signal();

        private BasicTitleBarButton _lockButton;


        protected override IWindowComponent Initialize()
        {
            Skin = WindowSkin;
            Draggable = true;
            Height = 1f;

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
            Height = minWindowSize.y;

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


        protected override void DrawWindow()
        {
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            GUILayout.Label("Experiments");
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            GUILayout.BeginScrollView(Vector2.zero, false, true);
            {
                for (int i = 0; i < 10; ++i)
                {
                    GUILayout.BeginHorizontal();
                    {
                        DrawRow("Experiment" + new string('.', i * 2));
                    }
                    GUILayout.EndHorizontal();
                    
         
                }
            }
            GUILayout.EndScrollView();
            GUILayout.Space(8f);
        }


        private void DrawRow(string buttonText)
        {
            GUILayout.Button(buttonText, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(false));
            //GUILayout.FlexibleSpace();

            GUILayout.Toggle(false, string.Empty, LitToggleStyle);
            GUILayout.Toggle(true, string.Empty, LitToggleStyle);

            //GUILayout.BeginHorizontal();
            //{
            //    GUILayout.Toggle(true, CloseButtonTexture, GUILayout.Width(CloseButtonTexture.width),
            //        GUILayout.Height(CloseButtonTexture.height));
            //    GUILayout.Toggle(false, CloseButtonTexture, GUILayout.Width(CloseButtonTexture.width),
            //        GUILayout.Height(CloseButtonTexture.height));
            //}
            //GUILayout.EndHorizontal();
        }


        private Vector2 CalculateMinRowSize()
        {
            string longest = "Experiment" + new string('.', 10 * 2);

            // button + toggle x2
            var button = Skin.button.CalcSize(new GUIContent(longest));
            var toggle = Skin.toggle.CalcSize(new GUIContent());

            return new Vector2(button.x + 2f * toggle.x, Mathf.Max(button.y, toggle.y));
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
