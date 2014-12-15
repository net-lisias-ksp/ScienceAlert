using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace ScienceAlert.ReeperCommon.Window
{
    class BasicWindow
    {
        protected IWindowLogic Logic;
        protected GUISkin Skin;
        public Rect WindowRect = new Rect();
        private int winId = UnityEngine.Random.Range(2444, int.MaxValue);


        protected void Awake()
        {
            
        }

        protected virtual void Render()
        {
            GUI.skin = WindowSkin;


            WindowRect = GUILayout.Window(winId, WindowRect, DoRender, Title);
        }

        private void DoRender(int winid)
        {

            if (!Logic.IsNull()) Logic.OnGUI();
        }

        protected virtual void Update()
        {
            if (!Logic.IsNull()) Logic.Update();
        }

        protected void OnDestroy()
        {
            
        }

        public GUISkin WindowSkin
        {
            get { return Skin ?? DefaultWindowSkin; }
            set
            {
                if (!Skin.IsNull())
                    GUISkin.Destroy(Skin);
                Skin = value;
            }
        }

        public IWindowLogic WindowLogic
        {
            get { return Logic; }
            set
            {
                if (!Logic.IsNull())
                    Logic.OnWindowUnassigned();

                Logic = value;
                if (!Logic.IsNull())
                    Logic.OnWindowAssigned();
            }
        }


        public string Title { get; set; }

        public static GUISkin DefaultWindowSkin
        {
            get;
            set;
        }

    }
}
