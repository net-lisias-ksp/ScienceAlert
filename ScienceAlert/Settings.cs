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
using System.Text;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;
using ReeperCommon;

namespace ScienceAlert
{
    public delegate void SaveCallback(ConfigNode node);

    public class Settings
    {
        // Singleton pattern
        private static Settings instance;

        public enum WarpSetting : int
        {
            ByExperiment = 0,
            GlobalOn,
            GlobalOff
        }

        public enum SoundNotifySetting : int
        {
            ByExperiment = 0,
            Always,
            Never
        }

        public enum ScanInterface : int
        {
            None = 0,               // experiment status updates always allowed
            ScanSat = 1             // Use Scansat interface if available
        }


        public enum ToolbarInterface : int
        {
            ApplicationLauncher = 0,
            BlizzyToolbar
        }


        
        private GUISkin skin;
        public event SaveCallback OnSave = delegate(ConfigNode node) { };



/******************************************************************************
 *                      Implementation details
 *****************************************************************************/
        private Settings()
        {
            // set default values
                skin = GameObject.Instantiate(HighLogic.Skin) as GUISkin;

                // adjust the skin a little bit.  It wastes a lot of space in its
                // current form

                    skin.button.fixedHeight = 24f;
                    skin.button.padding = new RectOffset() { left = 2, right = 2, top = 0, bottom = 0 };
                    skin.button.border = new RectOffset() { left = 2, right = 2, top = 1, bottom = 1 };

                    skin.toggle.border.top = skin.toggle.border.bottom = skin.toggle.border.left = skin.toggle.border.right = 0;
                    //skin.toggle.padding.left = skin.toggle.padding.right = skin.toggle.padding.top = skin.toggle.padding.bottom = 0;
                    skin.toggle.margin = new RectOffset(5, 0, 0, 0);
                    skin.toggle.padding = new RectOffset() { left = 5, top = 3, right = 3, bottom = 3 };

                    skin.box.alignment = TextAnchor.MiddleCenter;
                    skin.box.padding = new RectOffset(2, 2, 8, 5);
                    skin.box.contentOffset = new Vector2(0, 0f);

                    //skin.label.fontSize = skin.label.fontSize - 2;
                    skin.horizontalSlider.margin = new RectOffset();


                    // make the window background opaque
                    WindowOpacity = 255;

//                    Texture2D tex = skin.window.normal.background.CreateReadable();

//#if DEBUG
//                    tex.SaveToDisk("unmodified_window_bkg.png");
//#endif

//                    var pixels = tex.GetPixels32();

//                    for (int i = 0; i < pixels.Length; ++i)
//                        pixels[i].a = 255;

//                    tex.SetPixels32(pixels); tex.Apply();
//#if DEBUG
//                    tex.SaveToDisk("opaque_window_bkg.png");
//#endif

                    //// one of these apparently fixes the right thing
                    //skin.window.onActive.background = 
                    //skin.window.onFocused.background = 
                    //skin.window.onNormal.background = 
                    //skin.window.onHover.background = 
                    //skin.window.active.background = 
                    //skin.window.focused.background = 
                    //skin.window.hover.background = 
                    //skin.window.normal.background = tex;

                    skin.window.onNormal.textColor =
                        skin.window.normal.textColor = XKCDColors.Green_Yellow;

                    skin.window.onHover.textColor =
                        skin.window.hover.textColor = XKCDColors.YellowishOrange;

                    skin.window.onFocused.textColor =
                        skin.window.focused.textColor = Color.red;

                    skin.window.onActive.textColor =
                        skin.window.active.textColor = Color.blue;

                    skin.window.fontSize = 12;

                // default sane values, just in case the config doesn't exist
                    EvaAtmospherePressureWarnThreshold = 0.00035;
                    EvaAtmosphereVelocityWarnThreshold = 30;

                    ScanInterfaceType = ScanInterface.None;
                    ShowReportValue = false;
                    EvaReportOnTop = false;
                    ReopenOnEva = false;
                    CheckSurfaceSampleNotEva = false;
                    DisplayCurrentBiome = false;

            ReeperCommon.Window.DraggableWindow.DefaultSkin = skin;

            Load();
        }




        public static Settings Instance
        {
            get 
            {
                if (instance == null)
                    instance = new Settings();

                return instance;
            }
        }


        


        private string GetConfigPath()
        {
            return ConfigUtil.GetDllDirectoryPath() + "/settings.cfg";
        }



        public void Load()
        {
            var path = GetConfigPath();

            Log.Normal("Loading settings from {0}", path);
            if (File.Exists(path))
            {
                ConfigNode node = ConfigNode.Load(path);

                if (node == null)
                {
                    Log.Error("Failed to load {0}", path);
                }
                else DoLoad(node);
            }
            else
            {
                Log.Error("Failed to find settings file {0}", path);

                // save default values, then
                Save();
            }
        }

        public void Save()
        {
            var path = GetConfigPath();

            Log.Normal("Saving settings to {0}", path);
            ConfigNode saved = new ConfigNode();
            DoSave(saved);

            Log.Debug("About to save: {0}", saved.ToString());
            saved.Save(path);
            Log.Debug("Saved to {0}", path);
        }


        public void DoLoad(ConfigNode node)
        {
            Log.Debug("Settings.load");
            bool resave = false;

            
#region general settings
            try
            {
                ConfigNode general = node.GetNode("GeneralSettings");
                if (general == null) general = node.AddNode(new ConfigNode("GeneralSettings"));

                Log.Debug("General node = {0}", general.ToString());

                //FlaskAnimationEnabled = ConfigUtil.Parse<bool>(general, "FlaskAnimationEnabled", true);
                //StarFlaskFrameRate = ConfigUtil.Parse<float>(general, "StarFlaskFrameRate", 24f);
                
                DebugMode = ConfigUtil.Parse<bool>(general, "DebugMode", false);
                GlobalWarp = ConfigUtil.ParseEnum<WarpSetting>(general, "GlobalWarp", WarpSetting.ByExperiment);
                SoundNotification = ConfigUtil.ParseEnum<SoundNotifySetting>(general, "SoundNotification", SoundNotifySetting.ByExperiment);
                //ShowReportValue = general.Parse<bool>("ShowReportValue", false);
                //DisplayCurrentBiome = general.Parse<bool>("DisplayCurrentBiome", false);

            }
            catch (Exception e)
            {
                Log.Error("Exception occurred while loading GeneralSettings section: {0}", e);
            }
#endregion

#region user interface settings

            // note to self: move into own config section
            try
            {
                ConfigNode general = node.GetNode("GeneralSettings");
                if (general == null) general = node.AddNode(new ConfigNode("GeneralSettings"));

                FlaskAnimationEnabled = ConfigUtil.Parse<bool>(general, "FlaskAnimationEnabled", true);
                StarFlaskFrameRate = ConfigUtil.Parse<float>(general, "StarFlaskFrameRate", 24f);

                ShowReportValue = general.Parse<bool>("ShowReportValue", false);
                DisplayCurrentBiome = general.Parse<bool>("DisplayCurrentBiome", false);

                WindowOpacity = general.Parse<int>("WindowOpacity", 255);
            }
            catch (Exception e)
            {
                Log.Error("Exception occurred while loading user interface GeneralSettings section: {0}", e);
            }

            #endregion

#region crewed vessel settings

            try
            {
                ConfigNode crewed = node.GetNode("CrewedVesselSettings");
                if (crewed == null) crewed = node.AddNode(new ConfigNode("CrewedVesselSettings"));

                ReopenOnEva = crewed.Parse<bool>("ReopenOnEva", false);
                EvaAtmospherePressureWarnThreshold = crewed.Parse<double>("EvaAtmosPressureThreshold", 0.00035);
                EvaAtmosphereVelocityWarnThreshold = crewed.Parse<float>("EvaAtmosVelocityThreshold", 30);
                EvaReportOnTop = crewed.Parse<bool>("EvaReportOnTop", false);
                CheckSurfaceSampleNotEva = crewed.Parse<bool>("CheckSurfaceSampleNotEva", false);

            }
            catch (Exception e)
            {
                Log.Error("Exception occurred while loading CrewedVesselSettings section: {0}", e);
            }

            #endregion

#region interface settings

            // scan interface
            try
            {
                ConfigNode si = node.GetNode("ScanInterface");
                if (si == null) si = node.AddNode(new ConfigNode("ScanInterface"));

                Log.Debug("ScanInterface node = {0}", si.ToString());

                ScanInterfaceType = ConfigUtil.ParseEnum<ScanInterface>(si, "InterfaceType", ScanInterface.None);
                Log.Debug("ScanInterface = {0}", ScanInterfaceType.ToString());
            } catch (Exception e)
            {
                Log.Error("Exception occurred while loading ScanInterface section: {0}", e);
            }


            // toolbar interface
            try
            {
                ConfigNode ti = node.GetNode("ToolbarInterface");
                if (ti == null) ti = node.AddNode(new ConfigNode("ToolbarInterface"));

                ToolbarInterfaceType = ti.ParseEnum<ToolbarInterface>("ToolbarType", ToolbarInterface.ApplicationLauncher);
                Log.Debug("ToolbarInterface = {0}", ToolbarInterfaceType);
            }
            catch (Exception e)
            {
                Log.Error("Exception occurred while loading ToolbarInterface section: {0}", e);
            }
#endregion

#region sound settings
            //try
            //{
            //    var audioNode = node.GetNode("AudioSettings");

            //    if (audioNode != null)
            //    {
            //        if (audioNode.nodes != null)
            //        {
            //            foreach (var audioSetting in audioNode.nodes.DistinctNames())
            //            {
            //                var settings = new SoundSettings();

            //                Log.Debug("Loading sound settings for '{0}'", audioSetting);
            //                settings.OnLoad(audioNode.GetNode(audioSetting));

            //                PerSoundSettings[audioSetting] = settings;
            //            }
            //        }
            //        else Log.Error("No individual audio nodes found in AudioSettings");
            //    }
            //    else Log.Error("No AudioSettings ConfigNode found in settings.cfg");
            //}
            //catch (Exception e)
            //{
            //    Log.Error("Exception occurred while loading AudioSettings section: {0}", e);
            //}
#endregion

#region log settings
            Log.LoadFrom(node);
#endregion

            if (resave)
            {
                Log.Debug("Resave flag set; re-saving settings");
                Save();
            }
        }

        public void DoSave(ConfigNode node)
        {
            try
            {
                Log.Debug("Settings.save");

                #region general settings
                ConfigNode general = node.AddNode(new ConfigNode("GeneralSettings"));


                
                general.AddValue("DebugMode", DebugMode);
                general.AddValue("GlobalWarp", GlobalWarp);
                general.AddValue("SoundNotification", SoundNotification);


                #endregion

                #region user interface settings

                // note to self: move to own user interface section
                general.AddValue("ShowReportValue", ShowReportValue);
                general.AddValue("DisplayCurrentBiome", DisplayCurrentBiome);
                general.AddValue("FlaskAnimationEnabled", FlaskAnimationEnabled);
                general.AddValue("StarFlaskFrameRate", StarFlaskFrameRate);
                general.AddValue("WindowOpacity", WindowOpacity);

                #endregion

                #region crewed vessel settings
                ConfigNode crewed = node.AddNode(new ConfigNode("CrewedVesselSettings"));

                crewed.AddValue("EvaReportOnTop", EvaReportOnTop);
                crewed.AddValue("ReopenOnEva", ReopenOnEva);
                crewed.AddValue("EvaAtmosPressureThreshold", EvaAtmospherePressureWarnThreshold);
                crewed.AddValue("EvaAtmosVelocityThreshold", EvaAtmosphereVelocityWarnThreshold);
                crewed.AddValue("CheckSurfaceSampleNotEva", CheckSurfaceSampleNotEva);

                #endregion

                #region interface settings

                ConfigNode si = node.AddNode(new ConfigNode("ScanInterface"));

                si.AddValue("InterfaceType", ScanInterfaceType.ToString());

                node.AddNode(new ConfigNode("ToolbarInterface")).AddValue("ToolbarType", ToolbarInterfaceType.ToString());

                #endregion

                #region experiment settings
                //ConfigNode expSettings = node.AddNode(new ConfigNode("ExperimentSettings"));

                //foreach (var kvp in PerExperimentSettings)
                //    kvp.Value.OnSave(expSettings.AddNode(new ConfigNode(kvp.Key)));

                #endregion

                #region sound settings

                //var audioSettings = node.AddNode(new ConfigNode("AudioSettings"));

                //foreach (var kvp in PerSoundSettings)
                //{
                //    var n = audioSettings.AddNode(new ConfigNode(kvp.Key));
                //    kvp.Value.OnSave(n);
                //}

                #endregion

                #region log settings
                Log.SaveInto(node);
                #endregion

                Log.Debug("Finished saving settings");
            } catch (Exception e)
            {
                Log.Error("There was an exception while saving settings: {0}", e);
            }

            OnSave(node);
        }


        public static GUISkin Skin
        {
            get
            {
                return Settings.Instance.skin;
            }
        }


/******************************************************************************
* Settings
*****************************************************************************/
        #region General settings


        public bool DebugMode { get; private set; }
        public WarpSetting GlobalWarp { get; set; }
        public SoundNotifySetting SoundNotification { get; set; }


#endregion

        #region user interface settings

        public bool ShowReportValue { get; set; }
        public bool DisplayCurrentBiome { get; set; }
        public bool FlaskAnimationEnabled { get; set; }
        public float StarFlaskFrameRate { get; private set; }

        private int windowOpacity = 255;
        public int WindowOpacity
        {
            get
            {
                return windowOpacity;
            }

            set
            {
                if (value != windowOpacity)
                {
                    Texture2D tex = skin.window.normal.background.CreateReadable();

#if DEBUG
                    tex.SaveToDisk("unmodified_window_bkg.png");
#endif

                    var pixels = tex.GetPixels32();

                    for (int i = 0; i < pixels.Length; ++i)
                        pixels[i].a = (byte)(Mathf.Clamp(windowOpacity, 0, 255));

                    tex.SetPixels32(pixels); tex.Apply();
#if DEBUG
                    tex.SaveToDisk("usermodified_window_bkg.png");
#endif
                    // one of these apparently fixes the right thing
                    skin.window.onActive.background =
                    skin.window.onFocused.background =
                    skin.window.onNormal.background =
                    skin.window.onHover.background =
                    skin.window.active.background =
                    skin.window.focused.background =
                    skin.window.hover.background =
                    skin.window.normal.background = tex;
                }

                windowOpacity = value;
            }
        }

        #endregion


        #region Crewed vessel settings

        public bool ReopenOnEva { get; set; }
        public bool EvaReportOnTop { get; set; }
        public double EvaAtmospherePressureWarnThreshold { get; private set; }
        public float EvaAtmosphereVelocityWarnThreshold { get; private set; }
        public bool CheckSurfaceSampleNotEva { get; set; }

        #endregion

        #region scan interface settings


        protected ScanInterface Interface;
        protected ToolbarInterface ToolbarType;

        public ScanInterface ScanInterfaceType
        {
            get
            {
                // confirm that the given interface type does exist
                switch (Interface)
                {
                    case ScanInterface.ScanSat:
                        if (SCANsatInterface.IsAvailable())
                        {
                            return Interface;
                        }
                        else
                        {
#if DEBUG
                            Log.Error("Settings.ScanInterfaceType: SCANsatInterface unavailable");
#endif
                            return ScanInterface.None;
                        }
                    default:
                        return Interface;
                }
            }
            set
            {
                Interface = value;
            }
        }

        public ToolbarInterface ToolbarInterfaceType
        {
            get
            {
                switch (ToolbarType)
                {
                    case ToolbarInterface.BlizzyToolbar:
                        if (!ToolbarManager.ToolbarAvailable)
                            return ToolbarInterface.ApplicationLauncher;
                        return ToolbarInterface.BlizzyToolbar;

                    default:
                        return ToolbarType;
                }
            }

            set
            {
                ToolbarType = value;
                //Log.Debug("Settings.ToolbarType is now {0}", value.ToString());
            }
        }

        #endregion

        #region sound settings

        //public SoundSettings GetSoundSettings(string soundName)
        //{
        //    if (PerSoundSettings.ContainsKey(soundName))
        //    {
        //        return PerSoundSettings[soundName];
        //    } else {
        //        // return default settings
        //        Log.Debug("No loaded settings found for '{0}' -- creating default settings", soundName);

        //        var newSound = new SoundSettings();

        //        PerSoundSettings.Add(soundName, newSound);

        //        return newSound;
        //    }
        //}

        #endregion
    }


}
