using System;
using ReeperCommon.Gui.Window;
using ReeperCommon.Gui.Window.Buttons;
using ReeperCommon.Gui.Window.Decorators;
using strange.extensions.implicitBind;
using strange.extensions.injector;
using strange.extensions.signal.impl;
using UnityEngine;

namespace ScienceAlert.Gui
{
// ReSharper disable once ClassNeverInstantiated.Global
    [MediatedBy(typeof(AlertPanelMediator))]
    public class AlertPanelView : StrangeView
    {
        [Inject] public GUISkin WindowSkin { get; set; }

        [Inject(Keys.WindowTitleBarButtonStyle)] public GUIStyle TitleBarButtonStyle { get; set; }
        [Inject(Keys.CloseButtonTexture)] public Texture2D CloseButtonTexture { get; set; }
        [Inject(Keys.LockButtonTexture)] public Texture2D LockButtonTexture { get; set; }
        [Inject(Keys.UnlockButtonTexture)] public Texture2D UnlockButtonTexture { get; set; }
        [Inject(Keys.ResizeCursorTexture)] public Texture2D ResizeCursorTexture { get; set; }

        internal readonly Signal Close = new Signal();
        internal readonly Signal LockToggle = new Signal();

        private BasicTitleBarButton _lockButton;

        protected override IWindowComponent Initialize()
        {
            Skin = WindowSkin;
            Draggable = true;
            Height = 1f;


            var scaling = new WindowScale(this, Vector2.one);

            var clamp = new ClampToScreen(scaling);

            var tb = ConfigureTitleBar(clamp);

            var resizable = new Resizable(tb, ResizableHotzoneSize, MinWindowSize, ResizeCursorTexture)
            {
                Title = "Alert Panel"
            };

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
            GUILayout.Label("Stuff goes here");
        }

        protected override void FinalizeWindow()
        {

        }


        private void OnCloseButton()
        {
            Close.Dispatch();
        }


        private void OnLockButton()
        {
            LockToggle.Dispatch();
        }


        public void Lock(bool tf)
        {
            Draggable = tf;

            _lockButton.Texture = tf ? LockButtonTexture : UnlockButtonTexture;
        }
    }
}
