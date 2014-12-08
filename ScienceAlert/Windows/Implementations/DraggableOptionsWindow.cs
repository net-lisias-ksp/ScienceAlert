using System;
using System.Collections.Generic;
using System.Linq;
using System.Globalization;
using ReeperCommon;
using ScienceAlert.ProfileData.Implementations;
using UnityEngine;

namespace ScienceAlert.Windows.Implementations
{
    using ProfileManager = ScienceAlertProfileManager;
    //using ExperimentManager = Experiments.ExperimentManager;

    internal partial class DraggableOptionsWindow : ReeperCommon.Window.DraggableWindow
    {
        // --------------------------------------------------------------------
        //    Members
        // --------------------------------------------------------------------

        // Control position and scrollbars
        private Vector2 scrollPos = new Vector2();                  // scrollbar for profile experiment settings
        private Vector2 additionalScrollPos = new Vector2();        // scrollbar for additional options window
        private Vector2 profileScrollPos = Vector2.zero;            // scrollbar for profile list window

        private Dictionary<string /* expid */, int /* selected index */> experimentIds = new Dictionary<string, int>();
        private List<GUIContent> filterList = new List<GUIContent>();
        private string thresholdValue = "0";                        // temporary threshold string

        internal enum OpenPane
        {
            None,
            AdditionalOptions,
            LoadProfiles
        }

        private OpenPane submenu = OpenPane.None;



        // Materials and textures
        Texture2D collapseButton = new Texture2D(24, 24);
        Texture2D expandButton = new Texture2D(24, 24);
        Texture2D openButton = new Texture2D(24, 24);
        Texture2D saveButton = new Texture2D(24, 24);
        Texture2D returnButton = new Texture2D(24, 24);
        Texture2D deleteButton = new Texture2D(24, 24);
        Texture2D renameButton = new Texture2D(24, 24);
        Texture2D blackPixel = new Texture2D(1, 1);
        GUISkin whiteLabel;

        // locale
        NumberFormatInfo formatter;


        // gui styles and skins
        GUIStyle miniLabelLeft; // left justified
        GUIStyle miniLabelRight; // right justified
        GUIStyle miniLabelCenter; 

        // audio
        new AudioPlayer audio;

        /******************************************************************************
         *                    Implementation Details
         ******************************************************************************/

        protected override Rect Setup()
        {
            // position blocker in front of ApplicationLauncher buttons. The window is going to be drawn on
            // top of them regardless; this will just prevent us from accidentally interacting with them
            backstop.SetZ(ApplicationLauncher.Instance.anchor.transform.position.z - 50f);

            // culture setting
            Log.Normal("Configuring NumberFormatInfo for current locale");
            formatter = (NumberFormatInfo)NumberFormatInfo.CurrentInfo.Clone();
            formatter.CurrencySymbol = string.Empty;
            formatter.CurrencyDecimalDigits = 2;
            formatter.NumberDecimalDigits = 2;
            formatter.PercentDecimalDigits = 2;


            audio = AudioPlayer.Audio;

            if (audio == null) Log.Error("DraggableOptionsWindow: Failed to find AudioPlayer instance");


            var rawIds = ResearchAndDevelopment.GetExperimentIDs();
            var sortedIds = rawIds.OrderBy(expid => ResearchAndDevelopment.GetExperiment(expid).experimentTitle);

            Log.Debug("OptionsWindow: sorted {0} experiment IDs", sortedIds.Count());

            foreach (var id in sortedIds)
            {
                experimentIds.Add(id, (int)Convert.ChangeType(ProfileManager.ActiveProfile[id].Filter, ProfileManager.ActiveProfile[id].Filter.GetTypeCode()));
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


            openButton = ResourceUtil.GetEmbeddedTexture("ScienceAlert.Resources.btnOpen.png", false);
            saveButton = ResourceUtil.GetEmbeddedTexture("ScienceAlert.Resources.btnSave.png", false);
            returnButton = ResourceUtil.GetEmbeddedTexture("ScienceAlert.Resources.btnReturn.png", false);
            deleteButton = ResourceUtil.GetEmbeddedTexture("ScienceAlert.Resources.btnDelete.png", false);
            renameButton = ResourceUtil.GetEmbeddedTexture("ScienceAlert.Resources.btnRename.png", false);

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

            blackPixel.SetPixel(0, 0, Color.black); blackPixel.Apply();
            blackPixel.filterMode = FilterMode.Bilinear;

            whiteLabel = (GUISkin)GUISkin.Instantiate(Settings.Skin);
            whiteLabel.label.onNormal.textColor = Color.white;
            whiteLabel.toggle.onNormal.textColor = Color.white;
            whiteLabel.label.onActive.textColor = Color.white;

            submenu = OpenPane.None;
            Title = "ScienceAlert Options";

            // smaller label for less important text hints
            miniLabelLeft = new GUIStyle(Skin.label);
            miniLabelLeft.fontSize = 10;
            miniLabelLeft.normal.textColor = miniLabelLeft.onNormal.textColor = Color.white;

            miniLabelRight = new GUIStyle(miniLabelLeft);
            miniLabelRight.alignment = TextAnchor.MiddleRight;

            miniLabelCenter = new GUIStyle(miniLabelLeft);
            miniLabelCenter.alignment = TextAnchor.MiddleCenter;

            Settings.Instance.OnSave += OnAboutToSave;
            base.OnVisibilityChange += OnVisibilityChanged;
            GameEvents.onVesselChange.Add(OnVesselChanged);

            LoadFrom(Settings.Instance.additional.GetNode("OptionsWindow") ?? new ConfigNode());

            return new Rect(windowRect.x, windowRect.y, 324, Screen.height / 5 * 3);
        }

    
        new protected void OnDestroy()
        {
            base.OnDestroy();
            Log.Debug("DraggableOptionsWindow.OnDestroy");
            base.OnVisibilityChange -= OnVisibilityChanged;
        }


        #region events (and GameEvents)


        /// <summary>
        /// The only reason we're interested in vessel switches is to update the value of threshold; else
        /// it'll show the old one (although the slider is correct)
        /// </summary>
        /// <param name="newVessel"></param>
        //private void OnVesselChange(Vessel newVessel)
        //{
        //    Log.Debug("DraggableOptionsWindow.OnVesselChange to {0}", newVessel != null ? newVessel.vesselName : "<null>");
        //    thresholdValue = ProfileManager.ActiveProfile.ScienceThreshold.ToString("F2", formatter);
        //}

        private void OnVisibilityChanged(bool tf)
        {
            if (tf)
            {
                // just became visible; update threshold var
                thresholdValue = ProfileManager.ActiveProfile.ScienceThreshold.ToString("F2", formatter);
            }
        }


        private void OnVesselChanged(Vessel vessel)
        {
            OnVisibilityChanged(Visible);
        }

        protected override void OnCloseClick()
        {
            Visible = false;
        }


        /// <summary>
        /// Update window position in settings
        /// </summary>
        private void OnAboutToSave()
        {
            Log.Verbose("DraggableOptionsWindow.OnAboutToSave");
            SaveInto(Settings.Instance.additional.GetNode("OptionsWindow") ?? Settings.Instance.additional.AddNode("OptionsWindow"));
        }


        #endregion

        #region user interface drawing


        protected override void DrawUI()
        {
            GUILayout.BeginVertical(GUILayout.ExpandWidth(true), GUILayout.Height(Screen.height / 5 * 3));
            {

                GUILayout.Label(new GUIContent("Global Warp Settings"), GUILayout.ExpandWidth(true));
                Settings.Instance.GlobalWarp = (Settings.WarpSetting)GUILayout.SelectionGrid((int)Settings.Instance.GlobalWarp, new GUIContent[] { new GUIContent("By Experiment"), new GUIContent("Globally on"), new GUIContent("Globally off") }, 3, GUILayout.ExpandWidth(false));

                GUILayout.Label(new GUIContent("Global Alert Sound"), GUILayout.ExpandWidth(true));
                Settings.Instance.SoundNotification = (Settings.SoundNotifySetting)GUILayout.SelectionGrid((int)Settings.Instance.SoundNotification, new GUIContent[] { new GUIContent("By Experiment"), new GUIContent("Always"), new GUIContent("Never") }, 3, GUILayout.ExpandWidth(false));

                GUILayout.Space(4f);

                GUILayout.BeginHorizontal();
                GUILayout.Label(new GUIContent("Additional Options"));
                GUILayout.FlexibleSpace();
                //additionalOptions = AudibleButton(new GUIContent(additionalOptions ? collapseButton : expandButton)) ? !additionalOptions : additionalOptions;

                if (AudibleButton(new GUIContent(submenu == OpenPane.AdditionalOptions ? collapseButton : expandButton)))
                    submenu = submenu == OpenPane.AdditionalOptions ? OpenPane.None : OpenPane.AdditionalOptions;

                GUILayout.EndHorizontal();

                switch (submenu)
                {
                    case OpenPane.None:
                        DrawProfileSettings();
                        break;

                    case OpenPane.AdditionalOptions:
                        DrawAdditionalOptions();
                        break;

                    case OpenPane.LoadProfiles:
                        DrawProfileList();
                        break;
                }
            }
            GUILayout.EndVertical();
        }




        /// <summary>
        /// Regular, non-profile specific additional configuration options
        /// </summary>
        private void DrawAdditionalOptions()
        {
            //GUI.skin = whiteLabel;

            //additionalScrollPos = GUILayout.BeginScrollView(additionalScrollPos, Settings.Skin.scrollView, GUILayout.ExpandHeight(true));
            additionalScrollPos = GUILayout.BeginScrollView(additionalScrollPos, GUILayout.ExpandHeight(true));
            {
                GUILayout.Space(4f);

                GUILayout.BeginVertical(GUILayout.ExpandHeight(true));
                {

                    #region Alert settings
                    {
                        GUILayout.Box("User Interface Settings", GUILayout.ExpandWidth(true));

                        //-----------------------------------------------------
                        // global flask animation
                        //-----------------------------------------------------
                        GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
                        {
                            GUILayout.Label("Globally Enable Animation", GUILayout.ExpandWidth(true));
                            Settings.Instance.FlaskAnimationEnabled = AudibleToggle(Settings.Instance.FlaskAnimationEnabled, string.Empty, null, new GUILayoutOption[] { GUILayout.ExpandWidth(false) });
                            if (!Settings.Instance.FlaskAnimationEnabled && API.ScienceAlert.Button.IsAnimating) API.ScienceAlert.Button.SetLit();
                        }
                        GUILayout.EndHorizontal();


                        //-----------------------------------------------------
                        // Display next report value in button
                        //-----------------------------------------------------
                        {
                            Settings.Instance.ShowReportValue = AudibleToggle(Settings.Instance.ShowReportValue, "Display Report Value");
                        }


                        //-----------------------------------------------------
                        // Display current biome in experiment list
                        //-----------------------------------------------------
                        {
                            Settings.Instance.DisplayCurrentBiome = AudibleToggle(Settings.Instance.DisplayCurrentBiome, "Display Biome in Experiment List");
                        }


                        //-----------------------------------------------------
                        // Interface window opacity
                        //-----------------------------------------------------
                        GUILayout.Label("Window Opacity");
                        GUILayout.BeginHorizontal();
                        GUILayout.Label("Less", miniLabelLeft);
                        GUILayout.FlexibleSpace();
                        GUILayout.Label("More", miniLabelRight);
                        GUILayout.EndHorizontal();
                        Settings.Instance.WindowOpacity = (int)GUILayout.HorizontalSlider(Settings.Instance.WindowOpacity, 0f, 255f, GUILayout.ExpandWidth(true), GUILayout.MaxHeight(16f));
                        GUILayout.Space(8f);
                    } // end alert settings
                    #endregion


                    #region scan interface options
                    // scan interface options
                    {
                        GUILayout.Box("Third-party Integration Options", GUILayout.ExpandWidth(true));

                        GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
                        {
                            var oldInterface = Settings.Instance.ScanInterfaceType;
                            var prevColor = GUI.color;

                            if (!SCANsatInterface.IsAvailable()) GUI.color = Color.red;

                            bool enableSCANinterface = AudibleToggle(Settings.Instance.ScanInterfaceType == Settings.ScanInterface.ScanSat, "Enable SCANsat integration", null, new GUILayoutOption[] { GUILayout.ExpandWidth(true) });

                            GUI.color = prevColor;

                            if (enableSCANinterface && oldInterface != Settings.ScanInterface.ScanSat) // Settings won't return SCANsatInterface as the set interface if it wasn't found
                                if (!SCANsatInterface.IsAvailable())
                                {
                                    PopupDialog.SpawnPopupDialog("SCANsat Not Found", "SCANsat was not found. You must install SCANsat to use this feature.", "Okay", false, Settings.Skin);
                                    enableSCANinterface = false;
                                }

                            Settings.Instance.ScanInterfaceType = enableSCANinterface ? Settings.ScanInterface.ScanSat : Settings.ScanInterface.None;

                            API.ScienceAlert.ScanInterfaceType = Settings.Instance.ScanInterfaceType;
                        }
                        GUILayout.EndHorizontal();
                    } // end scan interface options


                    // toolbar interface options
                    {
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

                            if (API.ScienceAlert.ToolbarType != Settings.Instance.ToolbarInterfaceType)
                                API.ScienceAlert.ToolbarType = Settings.Instance.ToolbarInterfaceType;
                        }
                        GUILayout.EndHorizontal();
                    } // end toolbar interface options
                    #endregion


                    #region crewed vessel settings
                    {
                        GUILayout.Box("Crewed Vessel Settings", GUILayout.ExpandWidth(true));

                        //Settings.Instance.ReopenOnEva = AudibleToggle(Settings.Instance.ReopenOnEva, "Re-open list on EVA");
                        //{ // eva report on top
                        //    var prev = Settings.Instance.EvaReportOnTop;

                        //    Settings.Instance.EvaReportOnTop = AudibleToggle(Settings.Instance.EvaReportOnTop, "List EVA report first");

                        //    if (Settings.Instance.EvaReportOnTop != prev)
                        //        manager.ScheduleRebuildObserverList();
                        //}

                        // Surface sample on vessel
                        {
                            var prev = Settings.Instance.CheckSurfaceSampleNotEva;

                            Settings.Instance.CheckSurfaceSampleNotEva = AudibleToggle(prev, "Track surface sample in vessel");

                            if (prev != Settings.Instance.CheckSurfaceSampleNotEva)
                                API.ScienceAlert.SensorManager.CreateSensorsForVessel();

                        }

                    } // end crewed vessel settings
                    #endregion

                }
                GUILayout.EndVertical();

                GUI.skin = Settings.Skin;
            }
            GUILayout.EndScrollView();
        }



        /// <summary>
        /// Draws modifyable settings for the current active profile, assuming one is
        /// active and valid
        /// </summary>
        private void DrawProfileSettings()
        {
            if (ProfileManager.HasActiveProfile)
            {
                //-----------------------------------------------------
                // Active profile header with buttons
                //-----------------------------------------------------
                #region active profile header

                GUILayout.BeginHorizontal();
                {
                    GUILayout.Box(string.Format("Profile: {0}", ProfileManager.ActiveProfile.DisplayName), GUILayout.ExpandHeight(true), GUILayout.ExpandWidth(true));

                    // rename profile button
                    if (AudibleButton(new GUIContent(renameButton), GUILayout.MaxWidth(24)))
                        SpawnRenamePopup(ProfileManager.ActiveProfile);

                    // Save profile (only enabled if profile was actually modified)
                    GUI.enabled = ProfileManager.ActiveProfile.Modified;
                    if (AudibleButton(new GUIContent(saveButton), GUILayout.MaxWidth(24)))
                    {
                        SaveCurrentProfile();
                    }
                    GUI.enabled = true;

                    // Open profile (always available, warn user if profile modified)
                    if (AudibleButton(new GUIContent(openButton), GUILayout.MaxWidth(24)))
                        submenu = OpenPane.LoadProfiles;

                }
                GUILayout.EndHorizontal();

                #endregion

                //-----------------------------------------------------
                // scrollview with experiment options
                //-----------------------------------------------------
                #region experiment scrollview

                scrollPos = GUILayout.BeginScrollView(scrollPos, Settings.Skin.scrollView);
                {
                    GUI.skin = Settings.Skin;
                    GUILayout.Space(4f);


                    //-----------------------------------------------------
                    // min threshold slider ui
                    //-----------------------------------------------------
                    #region min threshold slider
                    GUI.SetNextControlName("ThresholdHeader");
                    GUILayout.Box("Alert Threshold");

                    GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true), GUILayout.MinHeight(14f));
                    {
                        if (ProfileManager.ActiveProfile.ScienceThreshold > 0f)
                        {
                            GUILayout.Label(string.Format("Alert Threshold: {0}", ProfileManager.ActiveProfile.ScienceThreshold.ToString("F2", formatter)));
                        }
                        else
                        {
                            var prev = GUI.color;
                            GUI.color = XKCDColors.Salmon;
                            GUILayout.Label("(disabled)");
                            GUI.color = prev;
                        }

                        GUILayout.FlexibleSpace();


                        if (string.IsNullOrEmpty(thresholdValue)) thresholdValue = ProfileManager.ActiveProfile.ScienceThreshold.ToString("F2", formatter);

                        GUI.SetNextControlName("ThresholdText");
                        string result = GUILayout.TextField(thresholdValue, GUILayout.MinWidth(60f));

                        // let the player use escape to cancel focus. Otherwise I'm likely to get a slew of
                        // bug reports about having locked up their GUI since all actions are locked while
                        // the text field has focus
                        if (Event.current.keyCode == KeyCode.Escape)
                            GUI.FocusControl("ThresholdHeader"); // kinda hacky but it works

                        
                        if (GUI.GetNameOfFocusedControl() == "ThresholdText") // only use text field value if it's focused; if we don't
                        // do this, then it'll continuously overwrite the slider value
                        {
                            try
                            {
                                float parsed = float.Parse(result, formatter);
                                ProfileManager.ActiveProfile.ScienceThreshold = parsed;

                                thresholdValue = result;
                            }
                            catch (Exception) // just in case
                            {
                                Log.Debug("Failed to parse float from '{0}'", result);
                            }

                            if (!InputLockManager.IsLocked(ControlTypes.ACTIONS_ALL))
                                InputLockManager.SetControlLock(ControlTypes.ACTIONS_ALL, "ScienceAlertThreshold");
                        }
                        else if (InputLockManager.GetControlLock("ScienceAlertThreshold") != ControlTypes.None)
                            InputLockManager.RemoveControlLock("ScienceAlertThreshold");


                    }
                    GUILayout.EndHorizontal();


                    GUILayout.Space(3f); // otherwise the TextField will overlap the slider just slightly

                    // threshold slider
                    float newThreshold = GUILayout.HorizontalSlider(ProfileManager.ActiveProfile.ScienceThreshold, 0f, 100f, GUILayout.ExpandWidth(true), GUILayout.Height(14f));
                    if (Math.Abs(newThreshold - ProfileManager.ActiveProfile.ScienceThreshold) > 0.001f)
                    {
                        ProfileManager.ActiveProfile.ScienceThreshold = newThreshold;
                        thresholdValue = newThreshold.ToString("F2", formatter);
                    }


                    // slider min/max value display. Put under slider because I couldn't get it centered on the sides
                    // properly and it just looked strange
                    GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true), GUILayout.MaxHeight(10f));

                    GUILayout.Label("0", miniLabelLeft);
                    GUILayout.FlexibleSpace();
                    GUILayout.Label("Science Amount", miniLabelCenter);
                    GUILayout.FlexibleSpace();
                    GUILayout.Label("100", miniLabelRight);
                    GUILayout.EndHorizontal();

                    #endregion

                    GUILayout.Space(10f);

                    // individual experiment settings
                    var keys = new List<string>(experimentIds.Keys);

                    foreach (var key in keys)
                    {
                        GUILayout.Space(4f);

                        var settings = ProfileManager.ActiveProfile[key];

                        // "asteroidSample" isn't listed in ScienceDefs (has a simple title of "Sample")
                        //   note: band-aided this in ScienceAlert.Start; leaving this note here in case
                        //         just switching an experiment's title causes issues later
                        var title = ResearchAndDevelopment.GetExperiment(key).experimentTitle;
#if DEBUG
                        GUILayout.Box(title + string.Format(" ({0})", ResearchAndDevelopment.GetExperiment(key).id), GUILayout.ExpandWidth(true));
#else
                            GUILayout.Box(title, GUILayout.ExpandWidth(true));
#endif

                        settings.Enabled = AudibleToggle(settings.Enabled, "Enabled");
                        settings.AnimationOnDiscovery = AudibleToggle(settings.AnimationOnDiscovery, "Animation on discovery");
                        settings.SoundOnDiscovery = AudibleToggle(settings.SoundOnDiscovery, "Sound on discovery");
                        settings.StopWarpOnDiscovery = AudibleToggle(settings.StopWarpOnDiscovery, "Stop warp on discovery");

                        // only add the Assume Onboard option if the experiment isn't
                        // one of the default types
                        //if (!settings.IsDefault)
                            //settings.AssumeOnboard = AudibleToggle(settings.AssumeOnboard, "Assume onboard");

                        GUILayout.Label(new GUIContent("Filter Method"), GUILayout.ExpandWidth(true), GUILayout.MinHeight(24f));

                        int oldSel = experimentIds[key];
                        experimentIds[key] = AudibleSelectionGrid(oldSel, ref settings);

                        if (oldSel != experimentIds[key])
                            Log.Debug("Changed filter mode for {0} to {1}", key, settings.Filter);


                    }
                }
                GUILayout.EndScrollView();

                #endregion
            }
            else
            { // no active profile
                GUI.color = Color.red;
                GUILayout.Label("No profile active");
            }
        }



        private void DrawProfileList()
        {
            profileScrollPos = GUILayout.BeginScrollView(profileScrollPos, Settings.Skin.scrollView);
            {
                if (ProfileManager.Count > 0)
                {
                    //DrawProfileList_HorizontalDivider();
                    GUILayout.Label("Select a profile to load");
                    GUILayout.Box(blackPixel, GUILayout.ExpandWidth(true), GUILayout.MinHeight(1f), GUILayout.MaxHeight(3f));
                    GUILayout.Space(4f); // just a bit of space to make the bar we drew stand out more

                    var profileList = ProfileManager.Profiles;

                    // always draw default profile first
                    DrawProfileList_ListItem(ProfileManager.DefaultProfile);

                    foreach (Profile profile in profileList.Values)
                        if (profile != ProfileManager.DefaultProfile)
                            DrawProfileList_ListItem(profile);

                }
                else // no profiles saved
                {
                    GUILayout.FlexibleSpace();
                    GUILayout.Box("No profiles saved", GUILayout.MinHeight(64f));
                    GUILayout.FlexibleSpace();
                }
            }
            GUILayout.Space(10f);
            GUILayout.BeginHorizontal();
            {
                GUILayout.FlexibleSpace();
                if (AudibleButton(new GUIContent("Cancel", "Cancel load operation"))) submenu = OpenPane.None;
            }
            GUILayout.EndHorizontal();
            GUILayout.EndScrollView();
        }


        private void DrawProfileList_ListItem(Profile profile)
        {
            GUILayout.BeginHorizontal();
            {
                GUILayout.Box(profile.Name, GUILayout.ExpandWidth(true));

                // rename button
                GUI.enabled = profile != ProfileManager.DefaultProfile;
                if (AudibleButton(new GUIContent(renameButton), GUILayout.MaxWidth(24), GUILayout.MinWidth(24)))
                    SpawnRenamePopup(profile);

                // open button
                GUI.enabled = true;
                if (AudibleButton(new GUIContent(openButton), GUILayout.MaxWidth(24), GUILayout.MinWidth(24)))
                    SpawnOpenPopup(profile);

                // delete button
                GUI.enabled = profile != ProfileManager.DefaultProfile;
                if (AudibleButton(new GUIContent(deleteButton), GUILayout.MaxWidth(24), GUILayout.MinWidth(24)))
                    SpawnDeletePopup(profile);

                GUI.enabled = true;
            }
            GUILayout.EndHorizontal();
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
        private bool AudibleToggle(bool value, GUIContent content, GUIStyle style = null, GUILayoutOption[] options = null)
        {
            bool result = GUILayout.Toggle(value, content, style == null ? Settings.Skin.toggle : style, options);
            if (result != value)
            {
                audio.PlayUI("click1");

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
        private int AudibleSelectionGrid(int currentValue, ref ProfileData.SensorSettings settings)
        {
            int newValue = GUILayout.SelectionGrid(currentValue, filterList.ToArray(), 2, GUILayout.ExpandWidth(true));
            if (newValue != currentValue)
            {
                audio.PlayUI("click1");
                settings.Filter = (ProfileData.SensorSettings.FilterMethod)newValue;
            }

            return newValue;
        }



        /// <summary>
        /// Simple wrapper
        /// </summary>
        /// <param name="content"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        private bool AudibleButton(GUIContent content, params GUILayoutOption[] options)
        {
            bool pressed = GUILayout.Button(content, options);

            if (pressed)
                audio.PlayUI("click1");

            return pressed;
        }

        #endregion
    }
}
