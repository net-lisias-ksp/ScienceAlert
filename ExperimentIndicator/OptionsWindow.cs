/******************************************************************************
 *                 Experiment Indicator for Kerbal Space Program              *
 *                                                                            *
 * Author: xEvilReeperx                                                       *
 *                                                                            *
 * ************************************************************************** *
 * Code licensed under the terms of GPL v3.0                                  *
 *                                                                            *
 * See the included LICENSE.txt or visit http://www.gnu.org/licenses/gpl.html *
 * for the full license text.                                                 *
 *                                                                            *
 *****************************************************************************/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Toolbar;
using DebugTools;

namespace ExperimentIndicator
{
    internal class OptionsWindow : IDrawable
    {
        private readonly int windowId = UnityEngine.Random.Range(0, int.MaxValue);

        // Control position and scrollbars
        private Rect windowRect;// = new Rect(0, 0, 256, Screen.height / 5 * 3);
        private Vector2 scrollPos = new Vector2();
        private bool drawFlag = false;
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

                //windowRect = new Rect(position.x, position.y, 256, Screen.height / 5 * 3);
                windowRect.x = position.x;
                windowRect.y = position.y;

                GUILayout.Window(windowId, windowRect, RenderControls, "Experiment Indicator");

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
                    settings.AssumeOnboard = AudibleToggle(settings.AssumeOnboard, "Assume onboard");

                    GUILayout.Label(new GUIContent("Filter Method"), GUILayout.ExpandWidth(true), GUILayout.MinHeight(24f));

                    int oldSel = experimentIds[key];
                    experimentIds[key] = AudibleSelectionGrid(oldSel, ref settings);

                    if (oldSel != experimentIds[key])
                        Log.Debug("Changed filter mode for {0} to {1}", key, settings.Filter);

                }

            GUILayout.EndScrollView();

            GUILayout.EndVertical();


            // trim the window, if necessary
            if (!drawFlag)
            {
                //windowRect.height = Math.Min(windowRect.height, GUILayoutUtility.GetLastRect().height);
                drawFlag = true;
            }

            //Log.Debug("Window rect = {0}", windowRect);
        }


        public void Update()
        {
            // empty
        }

    }
}
