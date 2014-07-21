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
using ReeperCommon;

namespace ScienceAlert
{
    /// <summary>
    /// It pretty much is what it sounds like
    /// </summary>
    internal class OptionsWindow : MonoBehaviour, IDrawable
    {
        private readonly int windowId = UnityEngine.Random.Range(0, int.MaxValue);

        // --------------------------------------------------------------------
        //    Members
        // --------------------------------------------------------------------

        // Control position and scrollbars
        private Rect windowRect;
        private Vector2 scrollPos = new Vector2();
        private Vector2 additionalScrollPos = new Vector2();
        private Dictionary<string /* expid */, int /* selected index */> experimentIds = new Dictionary<string, int>();
        private List<GUIContent> filterList = new List<GUIContent>();
        private string sciMinValue = "0";
        private bool additionalOptions = false; // flag set when additional options subwindow is open
        private ScienceAlert scienceAlert;

        // Materials and textures
        Texture2D collapseButton = new Texture2D(24, 24);
        Texture2D expandButton = new Texture2D(24, 24);
        GUISkin whiteLabel;

/******************************************************************************
 *                    Implementation Details
 ******************************************************************************/

        void Awake()
        {
            scienceAlert = gameObject.GetComponent<ScienceAlert>();

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

            //audio = audioDevice;
            sciMinValue = Settings.Instance.ScienceThreshold.ToString();

            var tex = ResourceUtil.GetEmbeddedTexture("ScienceAlert.Resources.btnExpand.png", false);

            if (tex == null)
            {
                Log.Error("Failed to retrieve expand button texture from stream");
            }
            else
            {
                Log.Debug("Collapse button texture loaded successfully");
                expandButton = tex;
                
                collapseButton = UnityEngine.Texture.Instantiate(expandButton) as Texture2D;
                ResourceUtil.FlipTexture(collapseButton, true, true);

                collapseButton.Compress(false);
                expandButton.Compress(false);
            }

            whiteLabel = (GUISkin)GUISkin.Instantiate(Settings.Skin);
            whiteLabel.label.onNormal.textColor = Color.white;
            whiteLabel.toggle.onNormal.textColor = Color.white;
            whiteLabel.label.onActive.textColor = Color.white;

            //redToggle = (GUISkin)GUISkin.Instantiate(Settings.Skin);
            //redToggle.toggle.onNormal.textColor =
            //redToggle.toggle.onHover.textColor =
            //redToggle.toggle.onActive.textColor = Color.red;
            //redToggle.toggle.normal.
        }



        void OnDestroy()
        {
            Log.Debug("OptionsWindow destroyed");
        }


        public void Update()
        {
            // required by IDrawable
        }


        System.Collections.IEnumerator WaitAndSave()
        {
            while (scienceAlert.Button.Drawable is OptionsWindow)
                yield return 0;

            Log.Normal("Saving settings");
            Settings.Instance.Save();
            Log.Normal("Settings saved.");
        }

        #region Events



        /// <summary>
        /// Called when ScienceAlert toolbar button was clicked
        /// </summary>
        /// <param name="ci"></param>
        public void OnToolbarClicked(Toolbar.ClickInfo ci)
        {
            if (scienceAlert.Button.Drawable == null)
            {
                if (ci.button == 1) // right-click
                {
                    if (scienceAlert.Button.Drawable is OptionsWindow)
                    {
                        scienceAlert.Button.Drawable = null;
                        AudioUtil.Play("click1");
                    }
                    else
                    {
                        scienceAlert.Button.Drawable = this;
                        StartCoroutine(WaitAndSave());
                        AudioUtil.Play("click1");
                    }
                }
            }
            else if (scienceAlert.Button.Drawable is OptionsWindow)
            {
                // we're open, non-right mouse button was clicked so close
                // the window
                scienceAlert.Button.Drawable = null;
                AudioUtil.Play("click1", 0.05f);
            }
        }

        #endregion



        #region GUI helper methods

        /// <summary>
        /// Helper method
        /// </summary>
        /// <param name="value"></param>
        /// <param name="content"></param>
        /// <param name="style"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        private bool AudibleToggle(bool value, string content, GUIStyle style = null, GUILayoutOption[] options = null)
        {
            return AudibleToggle(value, new GUIContent(content), style, options);
        }


        /// <summary>
        /// Just a wrapper around GUILayout.Toggle which plays a sound when
        /// its value changes.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="content"></param>
        /// <param name="style"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        private bool AudibleToggle(bool value, GUIContent content,  GUIStyle style = null, GUILayoutOption[] options = null)
        {
            bool result = GUILayout.Toggle(value, content, style == null ? Settings.Skin.toggle : style, options);
            if (result != value)
            {
                //audio.PlaySound("click1");
                AudioUtil.Play("click1");

#if DEBUG
                Log.Debug("Toggle '{0}' is now {1}", content.text, result);
#endif
            }
            return result;
        }


        /// <summary>
        /// Simple wrapper
        /// </summary>
        /// <param name="currentValue"></param>
        /// <param name="settings"></param>
        /// <returns></returns>
        private int AudibleSelectionGrid(int currentValue, ref Settings.ExperimentSettings settings)
        {
            int newValue = GUILayout.SelectionGrid(currentValue, filterList.ToArray(), 2, GUILayout.ExpandWidth(true));
            if (newValue != currentValue)
            {
                //audio.PlaySound("click1");
                AudioUtil.Play("click1");
                settings.Filter = (Settings.ExperimentSettings.FilterMethod)newValue;
            }

            return newValue;
        }


        /// <summary>
        /// Simple wrapper
        /// </summary>
        /// <param name="content"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        private bool AudibleButton(GUIContent content, GUILayoutOption[] options = null)
        {
            bool pressed = GUILayout.Button(content, options);

            if (pressed)
            {
                //audio.PlaySound("click1");
                AudioUtil.Play("click1");
            }

            return pressed;
        }

        #endregion


        #region Drawing functions

        /// <summary>
        /// Called by the toolbar button (whichever implementation) when it's
        /// time to draw the window.
        /// </summary>
        /// <param name="position"></param>
        /// <returns>Dimensions of rendered window</returns>
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

        private void RenderControls(int windowId)
        {
            GUILayout.BeginVertical(GUILayout.ExpandWidth(true), GUILayout.Height(windowRect.height));
            {

                GUILayout.Label(new GUIContent("Global Warp Settings"), GUILayout.ExpandWidth(true));
                Settings.Instance.GlobalWarp = (Settings.WarpSetting)GUILayout.SelectionGrid((int)Settings.Instance.GlobalWarp, new GUIContent[] { new GUIContent("By Experiment"), new GUIContent("Globally on"), new GUIContent("Globally off") }, 3, GUILayout.ExpandWidth(false));

                GUILayout.Label(new GUIContent("Global Alert Sound"), GUILayout.ExpandWidth(true));
                Settings.Instance.SoundNotification = (Settings.SoundNotifySetting)GUILayout.SelectionGrid((int)Settings.Instance.SoundNotification, new GUIContent[] { new GUIContent("By Experiment"), new GUIContent("Always"), new GUIContent("Never") }, 3, GUILayout.ExpandWidth(false));

                GUILayout.Space(4f);

                GUILayout.BeginHorizontal();
                    GUILayout.Label(new GUIContent("Additional Options"));
                    GUILayout.FlexibleSpace();
                    additionalOptions = AudibleButton(new GUIContent(additionalOptions ? collapseButton : expandButton)) ? !additionalOptions : additionalOptions;
                GUILayout.EndHorizontal();

                if (additionalOptions)
                {
                    GUI.skin = whiteLabel;

                    additionalScrollPos = GUILayout.BeginScrollView(additionalScrollPos, Settings.Skin.scrollView, GUILayout.ExpandHeight(true));
                    {
                        GUILayout.BeginVertical(GUILayout.ExpandHeight(true));
                        {
                            GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
                            {
                                GUILayout.Label("Globally Enable Animation", GUILayout.ExpandWidth(true));
                                Settings.Instance.FlaskAnimationEnabled = AudibleToggle(Settings.Instance.FlaskAnimationEnabled, string.Empty, null, new GUILayoutOption[] { GUILayout.ExpandWidth(false) });
                                if (!Settings.Instance.FlaskAnimationEnabled && scienceAlert.Button.IsAnimating) scienceAlert.Button.SetLit();
                            }
                            GUILayout.EndHorizontal();

                            GUILayout.Space(8f);


                            GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
                            {
                                GUILayout.Label(new GUIContent("Enable Minimum Science Threshold"), GUILayout.ExpandWidth(true));
                                Settings.Instance.EnableScienceThreshold = AudibleToggle(Settings.Instance.EnableScienceThreshold, string.Empty, null, new GUILayoutOption[] { GUILayout.ExpandWidth(false) });
                            }
                            GUILayout.EndHorizontal();

                            GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
                            {
                                GUILayout.Space(24f);
                                GUILayout.Label("Ignore reports worth less than:");
                                string newValue = GUILayout.TextField(sciMinValue, 5, GUILayout.MinWidth(24f));
                                newValue = newValue.Replace(',', '.');

                                float converted = 0f;

                                if (!string.Equals(sciMinValue, newValue))
                                    if (string.IsNullOrEmpty(newValue))
                                    {
                                        Settings.Instance.ScienceThreshold = 0f;
                                        sciMinValue = newValue;
                                    }
                                    else
                                    {
                                        if (float.TryParse(newValue, out converted))
                                        {
                                            Settings.Instance.ScienceThreshold = Mathf.Max(0f, converted);
                                            sciMinValue = newValue;

                                            Log.Debug("ScienceThreshold is now {0}", Settings.Instance.ScienceThreshold);
                                        }
                                        else
                                        {
                                            //audio.PlaySound("error");
                                            Log.Debug("Failed to convert '{0}' into a numeric value", newValue);
                                            Log.Debug("newValue = {0}, sciMinValue = {1}", newValue, sciMinValue);
                                        }
                                    }
                            }
                            GUILayout.EndHorizontal();
                            GUILayout.Space(10f);

                            // scan interface options
                            GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
                            {
                                var oldInterface = Settings.Instance.ScanInterfaceType;
                                var prevColor = GUI.color;

                                if (!SCANsatInterface.IsAvailable()) GUI.color = Color.red;

                                bool enableSCANinterface = AudibleToggle(Settings.Instance.ScanInterfaceType == Settings.ScanInterface.ScanSat, "Enable SCANsat integration", null, new GUILayoutOption[] { GUILayout.ExpandWidth(true) });

                                GUI.color = prevColor;

                                if (enableSCANinterface) // Settings won't return SCANsatInterface as the set interface if it wasn't found
                                    if (!SCANsatInterface.IsAvailable())
                                    {
                                        PopupDialog.SpawnPopupDialog("SCANsat Not Found", "SCANsat was not found. You must install SCANsat to use this feature.", "Okay", false, Settings.Skin);
#if DEBUG
                                    }
                                //else Log.Debug("SCANsatInterface is an available option");
#else
                                    }
#endif

                                Settings.Instance.ScanInterfaceType = enableSCANinterface ? Settings.ScanInterface.ScanSat : Settings.ScanInterface.None;

                                if (Settings.Instance.ScanInterfaceType != oldInterface)
                                    scienceAlert.ScheduleInterfaceChange(Settings.Instance.ScanInterfaceType);

                            }
                            GUILayout.EndHorizontal();

                            // toolbar interface option
                            GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
                            {
                                var oldInterface = Settings.Instance.ToolbarInterfaceType;
                                var prevColor = GUI.color;

                                if (!ToolbarManager.ToolbarAvailable) GUI.color = Color.red;

                                bool enableBlizzyToolbar = AudibleToggle(Settings.Instance.ToolbarInterfaceType == Settings.ToolbarInterface.BlizzyToolbar, "Use Blizzy toolbar");
                                GUI.color = prevColor;

                                if (enableBlizzyToolbar && oldInterface != Settings.ToolbarInterface.BlizzyToolbar)
                                    if (!ToolbarManager.ToolbarAvailable)
                                    {
                                        PopupDialog.SpawnPopupDialog("Blizzy Toolbar Not Found", "Blizzy's toolbar was not found. You must install Blizzy's toolbar to use this feature.", "Okay", false, Settings.Skin);
                                        enableBlizzyToolbar = false;
                                    }

                                Settings.Instance.ToolbarInterfaceType = enableBlizzyToolbar ? Settings.ToolbarInterface.BlizzyToolbar : Settings.ToolbarInterface.ApplicationLauncher;

                                if (scienceAlert.ToolbarType != Settings.Instance.ToolbarInterfaceType)
                                    scienceAlert.ToolbarType = Settings.Instance.ToolbarInterfaceType;
                            }
                            GUILayout.EndHorizontal();
                            GUILayout.Space(8f);

                            // Display report value in button
                            {
                                Settings.Instance.ShowReportValue = AudibleToggle(Settings.Instance.ShowReportValue, "Display Report Value");

                            }
                        }
                        GUILayout.EndVertical();

                        GUI.skin = Settings.Skin;
                    }
                    GUILayout.EndScrollView();
                }
                else // additional options not open
                {


                    scrollPos = GUILayout.BeginScrollView(scrollPos, Settings.Skin.scrollView);
                    {
                        GUI.skin = Settings.Skin;

                        var keys = new List<string>(experimentIds.Keys);

                        foreach (var key in keys)
                        {
                            GUILayout.Space(4f);

                            var settings = Settings.Instance.GetExperimentSettings(key);

                            // "asteroidSample" isn't listed in ScienceDefs (has a simple title of "Sample")
                            var title = ResearchAndDevelopment.GetExperiment(key).experimentTitle;
#if DEBUG
                            GUILayout.Box(title + string.Format(" ({0})", ResearchAndDevelopment.GetExperiment(key).id), GUILayout.ExpandWidth(true));
#else
                            GUILayout.Box(title, GUILayout.ExpandWidth(true));
#endif
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
                    }
                    GUILayout.EndScrollView();
                }
            }
            GUILayout.EndVertical();
        }

        #endregion

        #region Message handling functions

        /// <summary>
        /// This message will be sent by ScienceAlert when the user
        /// changes scan interface types
        /// </summary>
        public void Notify_ScanInterfaceChanged()
        {
            Log.Debug("OptionsWindow.Notify_ScanInterfaceChanged");
        }



        /// <summary>
        /// This message sent when toolbar has changed and re-registering
        /// for events is necessary
        /// </summary>
        public void Notify_ToolbarInterfaceChanged()
        {
            Log.Debug("OptionsWindow.Notify_ToolbarInterfaceChanged");
            scienceAlert.Button.OnClick += OnToolbarClicked;
        }

#endregion
    }
}
