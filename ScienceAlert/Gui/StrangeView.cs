using System;
using ReeperCommon.Extensions;
using ReeperCommon.Gui;
using ReeperCommon.Gui.Window;
using ReeperCommon.Gui.Window.Decorators;
using ReeperCommon.Serialization;
using strange.extensions.mediation.impl;
using UnityEngine;

namespace ScienceAlert.Gui
{
    public abstract class StrangeView : View, IWindowComponent
    {
        // ReSharper disable MemberCanBeProtected.Global
        // ReSharper disable UnusedAutoPropertyAccessor.Global
        // ReSharper disable RedundantDefaultFieldInitializer

        protected static readonly Vector2 TitleBarButtonOffset = new Vector2(2f, 2f);
        protected static readonly Vector2 ResizableHotzoneSize = new Vector2(10f, 10f);
        protected static readonly Vector2 MinWindowSize = new Vector2(200f, 100f);


        [ReeperPersistent] private Rect _windowRect = new Rect(0f, 0f, 200f, 300f);
        [ReeperPersistent] private WindowID _id = new WindowID();
        [ReeperPersistent] private string _title = string.Empty;
        [ReeperPersistent] private bool _draggable = false;
        [ReeperPersistent] private bool _visible = true;

        public IWindowComponent Decorated;


        protected override void Start()
        {
            base.Start();

            _id = new WindowID(GetInstanceID());
            Decorated = this; // just in case something goes wrong during init

            if (!registeredWithContext)
                throw new Exception("Failed to register with context");

            Decorated = Initialize() ?? this;
        }


        protected abstract IWindowComponent Initialize();
        protected abstract void DrawWindow();
        protected abstract void FinalizeWindow();


        #region window view


        // ReSharper disable once UnusedMember.Local
        // ReSharper disable once InconsistentNaming
        private void OnGUI()
        {

            if (Decorated.IsNull() || !Decorated.Visible) return;

            try
            {
                Decorated.OnWindowPreDraw();

                if (!Decorated.Skin.IsNull())
                    GUI.skin = Decorated.Skin;


                Decorated.Dimensions = GUILayout.Window(Decorated.Id.Value, Decorated.Dimensions, DrawWindow,
                    Decorated.Title, Skin.window);

                Decorated.OnWindowPostDraw();
            }
            catch (Exception e)
            {
                CloseWindowDueToError("Exception in StrangeView.OnGUI", e);
                throw;
            }
        }


        private void DrawWindow(int winid)
        {
            try
            {
                Decorated.OnWindowDraw(winid);
                Decorated.OnWindowFinalize(winid);
            }
            catch (Exception e)
            {
                CloseWindowDueToError("Exception in StrangeView.DrawWindow", e);
                throw;
            }
        }


        // ReSharper disable once UnusedMember.Local
        private void Update()
        {
            if (Decorated.IsNull()) return;

            Decorated.OnUpdate();
        }


        private void CloseWindowDueToError(string message, Exception e)
        {
            Visible = false;
            Debug.LogError(Title + ", id: " + Id +
               " has caused window to close: " + message + ", exception: " + e);
        }

        #endregion


        #region IWindowComponent

        public virtual void OnWindowPreDraw()
        {

        }

        public virtual void OnWindowDraw(int winid)
        {
            if (!Skin.IsNull()) GUI.skin = Skin;

            DrawWindow();
        }


        public virtual void OnWindowFinalize(int winid)
        {
            if (Draggable) GUI.DragWindow();
            FinalizeWindow();
        }


        public virtual void OnWindowPostDraw()
        {

        }
        public virtual void OnUpdate()
        {

        }

        public virtual void DuringSerialize(IConfigNodeSerializer formatter, ConfigNode node)
        {
            // this is kind of ugly, but since this object is actually decorated by others and we want
            // to capture their data as well and store it in our own (native) ConfigNode, we'll need some
            // special handling to make sure we don't end up in a loop 
            //
            // it's kinda unsafe to just mash them all into the one ConfigNode but doing it the "right" way
            // ends up in a whole lot of mostly empty nodes
            var current = Decorated;

            while (current is WindowDecorator)
            {
                formatter.WriteObjectToConfigNode(ref current, node);
                current = ((WindowDecorator)current).Decorated;
            }
        }


        public virtual void DuringDeserialize(IConfigNodeSerializer formatter, ConfigNode node)
        {
            var current = Decorated;

            while (current is WindowDecorator)
            {
                formatter.LoadObjectFromConfigNode(ref current, node);
                current = ((WindowDecorator)current).Decorated;
            }
        }


        public Rect Dimensions
        {
            get { return _windowRect; }
            set { _windowRect = value; }
        }


        public string Title
        {
            get { return _title; }
            set { _title = value; }
        }


        public GUISkin Skin { get; set; }


        public bool Draggable
        {
            get { return _draggable; }
            set { _draggable = value; }
        }


        public bool Visible
        {
            get { return _visible; }
            set { _visible = value; }
        }


        public WindowID Id
        {
            get { return _id; }
            // ReSharper disable once UnusedMember.Global
            set { _id = value; }
        }


        public float Width
        {
            get { return _windowRect.width; }
            set { _windowRect.width = value; }
        }

        public float Height
        {
            get { return _windowRect.height; }
            set { _windowRect.height = value; }
        }

        #endregion

    }
}
