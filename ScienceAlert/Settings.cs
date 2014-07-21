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


        // per-experiment settings
        public class ExperimentSettings
        {
            public enum FilterMethod : int
            {
                Unresearched = 0,                           // only light on subjects for which no science has been confirmed at all
                NotMaxed = 1,                               // light whenever the experiment subject isn't maxed
                LessThanFiftyPercent = 2,                   // less than 50% researched
                LessThanNinetyPercent = 3                   // less than 90% researched
            }


            public bool Enabled = true;
            public bool SoundOnDiscovery = true;
            public bool AnimationOnDiscovery = true;
            public bool StopWarpOnDiscovery = false;

            public FilterMethod Filter = FilterMethod.Unresearched;

            public bool AssumeOnboard = false;      // part of a workaround I'm thinking of for
                                                    // modded "experiments" that don't actually
                                                    // inherit from ModuleScienceExperiment
                                                    //
                                                    // Those I've looked at seem to define the biome and
                                                    // situation masks in ScienceDefs correctly, so although
                                                    // we won't be able to interact directly with the experiment,
                                                    // we should at least be able to tell when it could run under
                                                    // our desired filter
            public bool IsDefault = false;


            public void OnLoad(ConfigNode node)
            {
                Enabled = ConfigUtil.Parse<bool>(node, "Enabled", true);
                SoundOnDiscovery = ConfigUtil.Parse<bool>(node, "SoundOnDiscovery", true);
                AnimationOnDiscovery = ConfigUtil.Parse<bool>(node, "AnimationOnDiscovery", true);
                AssumeOnboard = ConfigUtil.Parse<bool>(node, "AssumeOnboard", false);
                StopWarpOnDiscovery = ConfigUtil.Parse<bool>(node, "StopWarpOnDiscovery", false);

                var strFilterName = node.GetValue("Filter");
                if (string.IsNullOrEmpty(strFilterName))
                {
                    Log.Error("Settings: invalid experiment filter");
                    strFilterName = Enum.GetValues(typeof(FilterMethod)).GetValue(0).ToString();
                }

                Filter = (FilterMethod)Enum.Parse(typeof(FilterMethod), strFilterName);
                IsDefault = ConfigUtil.Parse<bool>(node, "IsDefault", false);
            }

            public void OnSave(ConfigNode node)
            {
                node.AddValue("Enabled", Enabled);
                node.AddValue("SoundOnDiscovery", SoundOnDiscovery);
                node.AddValue("AnimationOnDiscovery", AnimationOnDiscovery);
                node.AddValue("StopWarpOnDiscovery", StopWarpOnDiscovery);
                node.AddValue("AssumeOnboard", AssumeOnboard);
                node.AddValue("Filter", Filter);
                node.AddValue("IsDefault", IsDefault);
            }

            public override string ToString()
            {
                ConfigNode node = new ConfigNode();
                OnSave(node);
                return node.ToString();
            }
        }





        private Dictionary<string /* expid */, ExperimentSettings> PerExperimentSettings;
        private GUISkin skin;

/******************************************************************************
 *                      Implementation details
 *****************************************************************************/
        private Settings()
        {
            // set default values
            PerExperimentSettings = new Dictionary<string, ExperimentSettings>();

            foreach (var expid in ResearchAndDevelopment.GetExperimentIDs())
                PerExperimentSettings[expid] = new ExperimentSettings();

            skin = GameObject.Instantiate(HighLogic.Skin) as GUISkin;

            // adjust the skin a little bit.  It wastes a lot of space in its
            // current form

                skin.button.fixedHeight = 24f;
                skin.button.padding = new RectOffset() { left = 2, right = 2, top = 2, bottom = 2 };

                skin.toggle.border.top = skin.toggle.border.bottom = skin.toggle.border.left = skin.toggle.border.right = 0;
                //skin.toggle.padding.left = skin.toggle.padding.right = skin.toggle.padding.top = skin.toggle.padding.bottom = 0;
                skin.toggle.margin = new RectOffset(5, 0, 0, 0);
                skin.toggle.padding = new RectOffset() { left = 5, top = 3, right = 3, bottom = 3 };

                skin.box.alignment = TextAnchor.MiddleCenter;
                skin.box.padding = new RectOffset(2, 2, 8, 5);
                skin.box.contentOffset = new Vector2(0, 0f);

                //skin.label.fontSize = skin.label.fontSize - 2;
                

            // default sane values, just in case the config doesn't exist
                EvaAtmospherePressureWarnThreshold = 0.00035;
                EvaAtmosphereVelocityWarnThreshold = 30;

                ScanInterfaceType = ScanInterface.None;

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
            string path = KSPUtil.ApplicationRootPath + "/GameData/ScienceAlert/settings.cfg";
            return path;
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
                else OnLoad(node);
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
            OnSave(saved);

            Log.Debug("About to save: {0}", saved.ToString());
            saved.Save(path);
            Log.Debug("Saved to {0}", path);
        }


        public void OnLoad(ConfigNode node)
        {
            Log.Debug("Settings.load");
            bool resave = false;

#region general settings
            try
            {
                ConfigNode general = node.GetNode("GeneralSettings");
                if (general == null) general = node.AddNode(new ConfigNode("GeneralSettings"));

                Log.Debug("General node = {0}", general.ToString());

                FlaskAnimationEnabled = ConfigUtil.Parse<bool>(general, "FlaskAnimationEnabled", true);
                StarFlaskFrameRate = ConfigUtil.Parse<float>(general, "StarFlaskFrameRate", 24f);
                EvaAtmospherePressureWarnThreshold = ConfigUtil.Parse<double>(general, "EvaAtmosPressureThreshold", 0.00035);
                EvaAtmosphereVelocityWarnThreshold = ConfigUtil.Parse<float>(general, "EvaAtmosVelocityThreshold", 30);
                DebugMode = ConfigUtil.Parse<bool>(general, "DebugMode", false);
                GlobalWarp = ConfigUtil.ParseEnum<WarpSetting>(general, "GlobalWarp", WarpSetting.ByExperiment);
                SoundNotification = ConfigUtil.ParseEnum<SoundNotifySetting>(general, "SoundNotification", SoundNotifySetting.ByExperiment);
                EnableScienceThreshold = ConfigUtil.Parse<bool>(general, "EnableScienceThreshold", false);
                ScienceThreshold = ConfigUtil.Parse<float>(general, "ScienceThreshold", 0f);

                Log.Debug("FlaskAnimationEnabled = {0}", FlaskAnimationEnabled);
                Log.Debug("StarFlaskFrameRate = {0}", StarFlaskFrameRate);
            }
            catch (Exception e)
            {
                Log.Error("Exception occurred while loading GeneralSettings section: {0}", e);
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
#region experiment settings
            try
            {
                ConfigNode experimentSettings = node.GetNode("ExperimentSettings");

                foreach (var nodeName in experimentSettings.nodes.DistinctNames())
                {
                    Log.Debug("Settings: Parsing experiment node '{0}'", nodeName);

                    ConfigNode experimentNode = experimentSettings.GetNode(nodeName);
                    Log.Debug("Its contents: {0}", experimentNode.ToString());

                    if (!PerExperimentSettings.ContainsKey(nodeName))
                    {
                        Log.Error("No experiment named '{0}' located.", nodeName);
                    }
                    else
                    {
                        PerExperimentSettings[nodeName].OnLoad(experimentNode);
                        Log.Debug("OnLoad for {0}: enabled = {1}, sound = {2}, animation = {3}, assume onboard = {4}", nodeName, PerExperimentSettings[nodeName].Enabled, PerExperimentSettings[nodeName].SoundOnDiscovery, PerExperimentSettings[nodeName].AnimationOnDiscovery, PerExperimentSettings[nodeName].AssumeOnboard);
                    }
                }

                if (PerExperimentSettings.Keys.Count != experimentSettings.nodes.DistinctNames().GetLength(0))
                {
                    // we don't have a match for experiments in the config
                    // vs loaded experiments.  Save the default values to disk
                    // immediately so they can be hand-edited if necessary.
                    Log.Warning(@"
Experiment config count does not match number of available experiments.  
Re-saving config with default values for missing experiments.");

                    resave = true;
                }
            }
            catch (Exception e)
            {
                Log.Error("Exception occurred while loading ExperimentSettings section: {0}", e);
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

        public void OnSave(ConfigNode node)
        {
            try
            {
                Log.Debug("Settings.save");

                #region general settings
                ConfigNode general = node.AddNode(new ConfigNode("GeneralSettings"));

                general.AddValue("FlaskAnimationEnabled", FlaskAnimationEnabled);
                general.AddValue("StarFlaskFrameRate", StarFlaskFrameRate);
                general.AddValue("EvaAtmosPressureThreshold", EvaAtmospherePressureWarnThreshold);
                general.AddValue("EvaAtmosVelocityThreshold", EvaAtmosphereVelocityWarnThreshold);
                general.AddValue("DebugMode", DebugMode);
                general.AddValue("GlobalWarp", GlobalWarp);
                general.AddValue("SoundNotification", SoundNotification);
                general.AddValue("EnableScienceThreshold", EnableScienceThreshold);
                general.AddValue("ScienceThreshold", ScienceThreshold);
                #endregion

                #region interface settings

                ConfigNode si = node.AddNode(new ConfigNode("ScanInterface"));

                si.AddValue("InterfaceType", ScanInterfaceType.ToString());

                node.AddNode(new ConfigNode("ToolbarInterface")).AddValue("ToolbarType", ToolbarInterfaceType.ToString());

                #endregion

                #region experiment settings
                ConfigNode expSettings = node.AddNode(new ConfigNode("ExperimentSettings"));

                foreach (var kvp in PerExperimentSettings)
                    kvp.Value.OnSave(expSettings.AddNode(new ConfigNode(kvp.Key)));

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

        public bool FlaskAnimationEnabled { get; set; }
        public float StarFlaskFrameRate { get; private set; }
        public double EvaAtmospherePressureWarnThreshold { get; private set; }
        public float EvaAtmosphereVelocityWarnThreshold { get; private set; }
        public bool DebugMode { get; private set; }
        public WarpSetting GlobalWarp { get; set; }
        public SoundNotifySetting SoundNotification { get; set; }
        public bool EnableScienceThreshold { get; set; }
        public float ScienceThreshold { get; set; }

        #endregion

        #region interface settings


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
                        if (AssemblyLoader.loadedAssemblies
                            .SelectMany(loaded => loaded.assembly.GetExportedTypes())
                            .ToList().Exists(t => string.Equals(t.FullName, "SCANsat.SCANdata")))
                        {
                            return Interface;
                        }
                        else return ScanInterface.None;

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
                Log.Debug("Settings.ToolbarType is now {0}", value.ToString());
            }
        }

        #endregion

        #region experiment settings

        public ExperimentSettings GetExperimentSettings(string expid)
        {
            if (PerExperimentSettings.ContainsKey(expid))
            {
                return PerExperimentSettings[expid];
            }
            else
            {
                Log.Error("Settings.GetExperimentSettings: a request to get settings for {0} was made; experiment id is unrecognized.", expid);
                return null;
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
