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
    internal class OptionsWindow : IDrawable
    {
        private readonly int windowId = UnityEngine.Random.Range(0, int.MaxValue);

        // Control position and scrollbars
        private Rect windowRect;// = new Rect(0, 0, 256, Screen.height / 5 * 3);
        private Vector2 scrollPos = new Vector2();
        private AudioController audio;
        private Dictionary<string /* expid */, int /* selected index */> experimentIds = new Dictionary<string, int>();
        private List<GUIContent> filterList = new List<GUIContent>();


        public OptionsWindow(AudioController audioDevice)
        {
            windowRect = new Rect(0, 0, 324, Screen.height / 5 * 3);

            var rawIds = ResearchAndDevelopment.GetExperimentIDs();
            var sortedIds = rawIds.OrderBy(expid => ResearchAndDevelopment.GetExperiment(expid).experimentTitle);

            foreach (var id in sortedIds)
            {
                experimentIds.Add(id, (int)Convert.ChangeType(Settings.Instance.GetExperimentSettings(id).Filter, Settings.Instance.GetExperimentSettings(id).Filter.GetTypeCode()));
                Log.Debug("Settings: experimentId {0} has filter index {1}", id, experimentIds[id]);
            }

            /*
                Unresearched = 0,                           
                NotMaxed = 1,                               
                LessThanFiftyPercent = 2,                   
                LessThanNinetyPercent = 3    
             */
            filterList.Add(new GUIContent("Unresearched"));
            filterList.Add(new GUIContent("Not maxed"));
            filterList.Add(new GUIContent("< 50% collected"));
            filterList.Add(new GUIContent("< 90% collected"));

            audio = audioDevice;
        }

        ~OptionsWindow()
        {
            Log.Debug("OptionsWindow destroyed");
        }


        public Vector2 Draw(Vector2 position)
        {
            var oldSkin = GUI.skin;
            GUI.skin = Settings.Skin;

                windowRect.x = position.x;
                windowRect.y = position.y;

                GUILayout.Window(windowId, windowRect, RenderControls, "Science Alert");

            GUI.skin = oldSkin;

            return new Vector2(windowRect.width, windowRect.height);
        }


        private bool AudibleToggle(bool value, string content)
        {
            bool result = GUILayout.Toggle(value, content);
            if (result != value)
            {
                audio.PlaySound("click1");

#if DEBUG
                Log.Debug("Toggle '{0}' is now {1}", content, result);
#endif
            }
            return result;
        }

        private int AudibleSelectionGrid(int currentValue, ref Settings.ExperimentSettings settings)
        {
            int newValue = GUILayout.SelectionGrid(currentValue, filterList.ToArray(), 2, GUILayout.ExpandWidth(true));
            if (newValue != currentValue)
            {
                audio.PlaySound("click1");
                settings.Filter = (Settings.ExperimentSettings.FilterMethod)newValue;
            }

            return newValue;
        }


        private void RenderControls(int windowId)
        {
            GUILayout.BeginVertical(GUILayout.ExpandWidth(true));

            // general settings
            GUILayout.Label("General Settings", GUILayout.ExpandWidth(true));
            //GUILayout.Space(6f);

            Settings.Instance.SaveFlightSessionManeuverNodes = AudibleToggle(Settings.Instance.SaveFlightSessionManeuverNodes, "Save flight session maneuvers");
            Settings.Instance.FlaskAnimationEnabled = AudibleToggle(Settings.Instance.FlaskAnimationEnabled, "Flask animation enabled");
            Settings.Instance.SoundOnNewResearch = AudibleToggle(Settings.Instance.SoundOnNewResearch, "Sound on new research available");
                GUILayout.Label(new GUIContent("Global Warp Settings"), GUILayout.ExpandWidth(true));
                Settings.Instance.GlobalWarp = (Settings.WarpSetting)GUILayout.SelectionGrid((int)Settings.Instance.GlobalWarp, new GUIContent[] { new GUIContent("By Experiment"), new GUIContent("Globally on"), new GUIContent("Globally off") }, 3, GUILayout.ExpandWidth(false));
            GUILayout.Space(10f);

            scrollPos = GUILayout.BeginScrollView(scrollPos, Settings.Skin.scrollView);
            GUI.skin = Settings.Skin;

                var keys = new List<string>(experimentIds.Keys);

                foreach (var key in keys)
                {
                    GUILayout.Space(8f);

                    var settings = Settings.Instance.GetExperimentSettings(key);
                    GUILayout.Box(ResearchAndDevelopment.GetExperiment(key).experimentTitle, GUILayout.ExpandWidth(true));

                    //GUILayout.Space(4f);
                    settings.Enabled = AudibleToggle(settings.Enabled, "Enabled");
                    settings.AnimationOnDiscovery = AudibleToggle(settings.AnimationOnDiscovery, "Animation on discovery");
                    settings.SoundOnDiscovery = AudibleToggle(settings.SoundOnDiscovery, "Sound on discovery");
                    settings.StopWarpOnDiscovery = AudibleToggle(settings.StopWarpOnDiscovery, "Stop warp on discovery");

                    // only add the Assume Onboard option if the experiment isn't
                    // one of the default types
                    if (!settings.IsDefault)
                        settings.AssumeOnboard = AudibleToggle(settings.AssumeOnboard, "Assume onboard");

                    GUILayout.Label(new GUIContent("Filter Method"), GUILayout.ExpandWidth(true), GUILayout.MinHeight(24f));

                    int oldSel = experimentIds[key];
                    experimentIds[key] = AudibleSelectionGrid(oldSel, ref settings);

                    if (oldSel != experimentIds[key])
                        Log.Debug("Changed filter mode for {0} to {1}", key, settings.Filter);

                    
                }

            GUILayout.EndScrollView();

            GUILayout.EndVertical();
        }


        public void Update()
        {
            // empty
        }

    }
}
