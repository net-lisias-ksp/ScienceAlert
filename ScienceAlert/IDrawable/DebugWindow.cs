/******************************************************************************
                   Science Alert for Kerbal Space Program                    
 ******************************************************************************
    Copyright (C) 2014 Allen Mrazek (amrazek@hotmail.com)

    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program.  If not, see <http://www.gnu.org/licenses/>.
 *****************************************************************************/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Toolbar;
using DebugTools;

namespace ScienceAlert
{
    /// <summary>
    /// This is an extra window accessible via the toolbar + middle mouse
    /// that should help in tracking down issues.
    /// </summary>
    public class DebugWindow : IDrawable
    {
        private readonly int windowId = UnityEngine.Random.Range(0, int.MaxValue);
        private Rect windowRect = new Rect(0, 0, 324, Screen.height / 5);


        public Vector2 Draw(Vector2 position)
        {
            var oldSkin = GUI.skin;
            GUI.skin = Settings.Skin;

            windowRect.x = position.x;
            windowRect.y = position.y;

            GUILayout.Window(windowId, windowRect, RenderOptions, "Science Alert Debug Options");

            GUI.skin = oldSkin;

            return new Vector2(windowRect.width, windowRect.height);
        }


        /// <summary>
        /// List all onboard science data
        /// </summary>
        private void DumpScienceData()
        {
            var containers = FlightGlobals.ActiveVessel.FindPartModulesImplementing<IScienceDataContainer>();

            Log.Write("Dumping all science found in {0} containers aboard {1}", containers.Count, FlightGlobals.ActiveVessel.vesselName);

            foreach (var can in containers)
            {
                var storedData = can.GetData();

                foreach (var data in storedData)
                    Log.Write("{0} - {1}", data.subjectID, data.title);
            }

            Log.Write("End science dump.");
        }


        private void RenderOptions(int winid)
        {
            GUILayout.BeginVertical(GUILayout.ExpandWidth(true));
                GUILayout.Label("Debug Options", GUILayout.ExpandWidth(true));
                GUILayout.Space(10f);

                if (GUILayout.Button("Log Onboard ScienceData", GUILayout.ExpandWidth(true)))
                    DumpScienceData();

            GUILayout.EndVertical();
        }

        public void Update()
        {
            // required by IDrawable
        }
    }
}
