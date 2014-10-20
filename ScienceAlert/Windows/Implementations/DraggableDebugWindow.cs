///******************************************************************************
//                   Science Alert for Kerbal Space Program                    
// ******************************************************************************
//    Copyright (C) 2014 Allen Mrazek (amrazek@hotmail.com)

//    This program is free software: you can redistribute it and/or modify
//    it under the terms of the GNU General Public License as published by
//    the Free Software Foundation, either version 3 of the License, or
//    (at your option) any later version.

//    This program is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//    GNU General Public License for more details.

//    You should have received a copy of the GNU General Public License
//    along with this program.  If not, see <http://www.gnu.org/licenses/>.
// *****************************************************************************/
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
