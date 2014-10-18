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
            Title = "Debug";
            Skin = Settings.Skin;
            Settings.Instance.OnAboutToSave += AboutToSave;

            LoadFrom(Settings.Instance.additional.GetNode("DebugWindow") ?? new ConfigNode());
            return new Rect(windowRect.x, windowRect.y, 256f, 128f);
        }


        private void AboutToSave()
        {
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


        private void OnSettingsSave(ConfigNode node)
        {
            SaveInto(node.AddNode("DebugWindow"));
        }

        private void OnSettingsLoad(ConfigNode node)
        {
            LoadFrom(node.GetNode("DebugWindow") ?? new ConfigNode());
        }
    }
}
