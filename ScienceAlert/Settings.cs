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
using ReeperCommon.ConfigNodeSerialization;

//#error figure out a way to let DraggableWindows listen to events from Settings

namespace ScienceAlert
{
    

    public class Settings : IReeperSerializable
    {
        public delegate void SaveCallback(ConfigNode node);
        public delegate void Callback();

        // Singleton pattern
        private static Settings instance;

        [DoNotSerialize]
        private readonly string ConfigPath = ConfigUtil.GetDllDirectoryPath() + "/settings.cfg";

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


        [DoNotSerialize]
        private GUISkin skin;

        [field: DoNotSerialize] // note: specifier required, else it won't apply to compiler-generated field portion
        public event Callback OnSave = delegate() { };

        //[field: DoNotSerialize]
        //public event Callback OnAboutToSave = delegate() { };
        
        [field: DoNotSerialize]
        public event SaveCallback OnLoad = delegate(ConfigNode node) { };

        //[field: DoNotSerialize]
        //public event Callback OnAboutToLoad = delegate() { };

        /// <summary>
        /// Extra storage for objects that don't register for listeners in time
        /// </summary>
        public ConfigNode additional = new ConfigNode("config");


/******************************************************************************
 *                      Implementation details
 *****************************************************************************/
        /// <summary>
        /// Set up ScienceAlert skin and set default values; then load settings from cfg if 
        /// it exists
        /// </summary>
        private Settings()
        {
            
            // set default values
                // create custom skin (based on KSP skin)
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

                    skin.horizontalSlider.margin = new RectOffset();

                    // make the window background opaque by default
                    WindowOpacity = 255;

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
                CheckSurfaceSampleNotEva = false;
                DisplayCurrentBiome = false;
                StarFlaskFrameRate = 24f;
                FlaskAnimationEnabled = true;

            ReeperCommon.Window.DraggableWindow.DefaultSkin = skin;

            Load();
        }




        /// <summary>
        /// Singleton Instance
        /// </summary>
        public static Settings Instance
        {
            get
            {
                if (instance == null)
                    instance = new Settings();

                return instance;
            }
        }



        /// <summary>
        /// Currently unused; mainly to confirm that serialization is working as expected
        /// </summary>
        /// <param name="node"></param>
        public void OnSerialize(ConfigNode node)
        {
            Log.Verbose("Settings.OnSerialize");
        }



        /// <summary>
        /// Currently unused; mainly to confirm that serialization is working as expected
        /// </summary>
        /// <param name="node"></param>
        public void OnDeserialize(ConfigNode node)
        {
            Log.Verbose("Settings.OnDeserialize");
        }




        /// <summary>
        /// Loads settings from cfg
        /// </summary>
        public void Load()
        {
 
            Log.Normal("Loading settings from {0}", ConfigPath);
            if (File.Exists(ConfigPath))
            {
                ConfigNode node = ConfigNode.Load(ConfigPath);

                if (node == null)
                {
                    Log.Error("Failed to load {0}", ConfigPath);
                }
                else 
                {
                    node.CreateObjectFromConfigEx(this);
                    Log.LoadFrom(node);

                    Log.Debug("Settings.additional = {0}", additional.ToString());

                    OnLoad(additional);
                }
            }
            else
            {
                Log.Error("Failed to find settings file {0}", ConfigPath);

                // save default values, then
                Save();
            }
        }



        /// <summary>
        /// Saves settings to cfg
        /// </summary>
        public void Save()
        {
            ConfigNode saved = null;

            try
            {
                OnSave();

                saved = this.CreateConfigFromObjectEx() ?? new ConfigNode();
            }
            catch (Exception e)
            {
                Log.Error("Exception while creating ConfigNode from settings: {0}", e);
            }

            Log.SaveInto(saved);

            Log.Debug("About to save: {0}", saved.ToString());


            // note: it's really important we not save an empty ConfigNode to disk, else
            // it'll stall KSP next time it loads
            if (saved.CountNodes > 0 || saved.CountValues > 0)
            {

                Log.Normal("Saving settings to {0}", ConfigPath);
                saved.Save(ConfigPath);
            }
            else
            {
                Log.Warning("Settings.Save: ConfigNode looks empty. Saving this would lead to problems. All values will be reset to default");
                Log.Warning("Problem ConfigNode: {0}", saved.ToString());
            }
        }



        /// <summary>
        /// Customized GUISkin for ScienceAlert (mainly some adjustments to make space usage
        /// a bit more efficient)
        /// </summary>
        public static GUISkin Skin
        {
            get
            {
                return Settings.Instance.skin;
            }
        }


#region Debug settings


        /// <summary>
        /// Returns true if the debug window should be available
        /// </summary>
        [HelpDoc("Enables middle-click debug window")]
        [Subsection("Debug")]
        public bool DebugMode { get; private set; }




#endregion


#region general settings

        /// <summary>
        /// Global warp setting; if not "ByExperiment" then individual experiment settings
        /// will be ignored and this setting will be used instead
        /// </summary>
        [Subsection("General")]
        public WarpSetting GlobalWarp { get; set; }



        /// <summary>
        /// Global sound setting; if not "ByExperiment" then individual experiment settings
        /// will be ignored and this setting used instead
        /// </summary>
        [Subsection("General")]
        public SoundNotifySetting SoundNotification { get; set; }


        [Subsection("General")]
        public double EvaAtmospherePressureWarnThreshold { get; private set; }


        [Subsection("General")]
        public float EvaAtmosphereVelocityWarnThreshold { get; private set; }

#endregion


#region User interface settings

        /// <summary>
        /// Display next science report value in deploy button?
        /// </summary>
        [Subsection("UserInterface")]
        public bool ShowReportValue { get; set; }



        /// <summary>
        /// Display current biome instead of experiment list's window title?
        /// </summary>
        [Subsection("UserInterface")]
        public bool DisplayCurrentBiome { get; set; }



        /// <summary>
        /// Should we animate the flask? The star flask will still appear, it just won't rotate
        /// if set false
        /// </summary>
        [Subsection("UserInterface")]
        public bool FlaskAnimationEnabled { get; set; }



        /// <summary>
        /// Frame rate (per second) of the star flask animation
        /// </summary>
        [Subsection("UserInterface")]
        public float StarFlaskFrameRate { get; private set; }


        /// <summary>
        /// Backing field for WindowOpacity property; not serialized because we want
        /// the serialization method to trigger logic inside the property instead
        /// </summary>
        [DoNotSerialize]
        private int windowOpacity = 255;


        /// <summary>
        /// Window opacity, 0 = transparent while 255 = opaque
        /// </summary>
        [HelpDoc("Window translucency; 0 = transparent, 255 = completely opaque")]
        [Subsection("UserInterface")]
        public int WindowOpacity
        {
            get
            {
                return windowOpacity;
            }

            set
            {

                Texture2D tex = skin.window.normal.background.CreateReadable();
                windowOpacity = value;

                //#if DEBUG
                //                Log.Debug("WindowOpacity set to " + value);
                //                tex.SaveToDisk("unmodified_window_bkg.png");
                //#endif

                var pixels = tex.GetPixels32();

                for (int i = 0; i < pixels.Length; ++i)
                    pixels[i].a = (byte)(Mathf.Clamp(windowOpacity, 0, 255));

                tex.SetPixels32(pixels); tex.Apply();
                //#if DEBUG
                //                tex.SaveToDisk("usermodified_window_bkg.png");
                //#endif
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
        }



#endregion





#region Crewed vessel settings


        /// <summary>
        /// If true, EVA reports will always be listed first in the experiment list window
        /// </summary>
        [HelpDoc("Should EVA reports always be on top of experiment list?")]
        [Subsection("CrewedVesselSettings")]
        public bool EvaReportOnTop { get; set; }


        [Subsection("CrewedVesselSettings")]
        public bool CheckSurfaceSampleNotEva { get; set; }

#endregion

#region scan interface settings

        [DoNotSerialize] // use property logic
        protected ScanInterface Interface;

        [DoNotSerialize] // use property logic
        protected ToolbarInterface ToolbarType;

        [HelpDoc("Valid options: ScanSat, None")]
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

        [HelpDoc("Valid options: BlizzyToolbar, ApplicationLauncher")]
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
            }
        }

#endregion
    }


}
