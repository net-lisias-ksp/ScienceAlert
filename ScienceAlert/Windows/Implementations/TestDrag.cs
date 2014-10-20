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
using UnityEngine;
using ReeperCommon;

namespace ScienceAlert.Windows.Implementations
{
    class TestDrag : ReeperCommon.Window.DraggableWindow
    {

        protected override Rect Setup()
        {
            Log.Write("TestDrag.Setup");

            //backstop.SetZ(MessageSystem.Instance.hoverComponent.transform.position.z);

            return new Rect(300, 300, 300, 300);
        }

        protected override void DrawUI()
        {
            //Log.Write("TestDrag.DrawUI");
            GUILayout.BeginVertical();
            GUILayout.Label("TestDrag.DrawUI");
            GUILayout.EndVertical();
        }

        protected override void OnCloseClick()
        {
            Log.Warning("Window close attempt");
        }
    }
}
