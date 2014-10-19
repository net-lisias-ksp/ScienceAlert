using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ReeperCommon;
using UnityEngine;

namespace ScienceAlert.Windows.Implementations
{
    class DraggableDebugWindow : ReeperCommon.Window.DraggableWindow
    {

        /// <summary>
        /// DraggableWindow setup here
        /// </summary>
        /// <returns></returns>
        protected override Rect Setup()
        {
            // position blocker in front of ApplicationLauncher buttons. The window is going to be drawn on
            // top of them regardless; this will just prevent us from accidentally interacting with them
            backstop.SetZ(ApplicationLauncher.Instance.anchor.transform.position.z - 50f);

            Title = "Debug";
            Skin = Settings.Skin;
            Settings.Instance.OnSave += AboutToSave;

            LoadFrom(Settings.Instance.additional.GetNode("DebugWindow") ?? new ConfigNode());

            Log.Debug("DraggableDebugWindow.Setup");

            return new Rect(windowRect.x, windowRect.y, 256f, 128f);
        }


        private void AboutToSave()
        {
            Log.Debug("DraggableDebugWindow.AboutToSave");
            SaveInto(Settings.Instance.additional.GetNode("DebugWindow") ?? Settings.Instance.additional.AddNode("DebugWindow"));
        }



        protected override void DrawUI()
        {
            GUILayout.BeginVertical(GUILayout.ExpandHeight(true), GUILayout.MinHeight(128f));

            // current biome
            GUILayout.Label("Biome: to be implemented", GUILayout.MinWidth(256f));

            GUILayout.EndVertical();
        }



        protected override void OnCloseClick()
        {
            Visible = false;
        }


    }
}
