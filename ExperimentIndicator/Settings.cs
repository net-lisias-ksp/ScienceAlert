using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Reflection;
using UnityEngine;

namespace ExperimentIndicator
{
    internal class Settings
    {
        // Singleton pattern
        private static Settings instance;


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

            public void OnLoad(ConfigNode node)
            {
                Enabled = Settings.Parse<bool>(node, "Enabled", true);
                SoundOnDiscovery = Settings.Parse<bool>(node, "SoundOnDiscovery", true);
                AnimationOnDiscovery = Settings.Parse<bool>(node, "AnimationOnDiscovery", true);
                AssumeOnboard = Settings.Parse<bool>(node, "AssumeOnboard", false);
                StopWarpOnDiscovery = Settings.Parse<bool>(node, "StopWarpOnDiscovery", false);

                var strFilterName = node.GetValue("Filter");
                if (string.IsNullOrEmpty(strFilterName))
                {
                    Log.Error("Settings: invalid experiment filter");
                    strFilterName = Enum.GetValues(typeof(FilterMethod)).GetValue(0).ToString();
                }

                Filter = (FilterMethod)Enum.Parse(typeof(FilterMethod), strFilterName);
            }

            public void OnSave(ConfigNode node)
            {
                node.AddValue("Enabled", Enabled);
                node.AddValue("SoundOnDiscovery", SoundOnDiscovery);
                node.AddValue("AnimationOnDiscovery", AnimationOnDiscovery);
                node.AddValue("StopWarpOnDiscovery", StopWarpOnDiscovery);
                node.AddValue("AssumeOnboard", AssumeOnboard);
                node.AddValue("Filter", Filter);
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

            SaveFlightSessionManeuverNodes = true;
            FlaskAnimationEnabled = true;
            SoundOnNewResearch = true;


            skin = GameObject.Instantiate(HighLogic.Skin) as GUISkin;

            // adjust the skin a little bit.  It wastes a lot of space in its
            // current form

            skin.button.fixedHeight = 24f;
            skin.toggle.border.top = skin.toggle.border.bottom = skin.toggle.border.left = skin.toggle.border.right = 0;
            skin.box.alignment = TextAnchor.MiddleCenter;
            skin.box.padding = new RectOffset(5, 5, 15, 10);
            skin.box.contentOffset = Vector2.zero;

            //windowSkin.scrollView.border.left = windowSkin.scrollView.border.right = windowSkin.scrollView.border.bottom = windowSkin.scrollView.border.top = 0;

            //windowSkin.scrollView.margin = new RectOffset(0, 8, 0, 0);

            //windowSkin.toggle.padding.top = windowSkin.toggle.padding.bottom = -2;
            //windowSkin.scrollView.padding.left = windowSkin.scrollView.padding.right = windowSkin.scrollView.padding.bottom = windowSkin.scrollView.padding.top = 0;

            //windowSkin.label.normal.background = blackBg;
            //windowSkin.label.alignment = TextAnchor.MiddleCenter;
            //windowSkin.toggle.alignment = TextAnchor.LowerCenter;



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
        
        
        public static T Parse<T>(ConfigNode node, string valueName, T defaultValue)
        {
            try
            {
                var value = node.GetValue(valueName);
                if (string.IsNullOrEmpty(value))
                {
                    Log.Error("Settings: Value '{0}' does not exist in given ConfigNode", valueName);
                    return defaultValue;
                }

                var method = typeof(T).GetMethod("TryParse", new[] {
                    typeof (string),
                    typeof(T).MakeByRefType()
                });

                if (method == null)
                {
                    Log.Error("Failed to locate TryParse in {0}", typeof(T).FullName);
                }
                else
                {
                    object[] args = new object[] { value, default(T) };

                    if ((bool)method.Invoke(null, args))
                    {
                        Log.Debug("Examined {0}, parse returned{1}", value, args[1]);
                        return (T)args[1];
                    }
                    else
                    {
                        Log.Error("Settings: TryParse failed with node name '{0}' (returned value '{1}'", valueName, string.IsNullOrEmpty(valueName) ? "[null]" : value);
                        return defaultValue;
                    }
                }
            }
            catch (Exception e)
            {
                Log.Error("Settings: Failed to parse value '{0}' from ConfigNode, resulted in an exception {1}", valueName, e);
            }

            return defaultValue;
        }


        private string GetConfigPath()
        {
            string path = KSPUtil.ApplicationRootPath + "/GameData/ExperimentIndicator/settings.cfg";
            return path;
        }

        public void Load()
        {
            var path = GetConfigPath();

            Log.Write("Loading settings from {0}", path);
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

            Log.Write("Saving settings to {0}", path);
            ConfigNode saved = new ConfigNode();
            OnSave(saved);
            saved.Save(path);
        }


        public void OnLoad(ConfigNode node)
        {
            Log.Debug("Settings.load");

#region general settings
            ConfigNode general = node.GetNode("GeneralSettings");

            Log.Debug("General node = {0}", general.ToString());

            SaveFlightSessionManeuverNodes = Parse<bool>(general, "SaveFlightSessionManeuverNodes", true);
            FlaskAnimationEnabled = Parse<bool>(general, "FlaskAnimationEnabled", true);
            SoundOnNewResearch = Parse<bool>(general, "SoundOnNewResearch", true);


            Log.Debug("SaveFlightSessionManeuverNodes = {0}", SaveFlightSessionManeuverNodes);
            Log.Debug("FlaskAnimationEnabled = {0}", FlaskAnimationEnabled);
            Log.Debug("SoundOnNewResearch = {0}", SoundOnNewResearch);

#endregion

#region experiment settings
            ConfigNode experimentSettings = node.GetNode("ExperimentSettings");

            foreach (var nodeName in experimentSettings.nodes.DistinctNames())
            {
                Log.Debug("Settings: Parsing experiment node '{0}'", nodeName);

                ConfigNode experimentNode = experimentSettings.GetNode(nodeName);
                Log.Debug("Its contents: {0}", experimentNode.ToString());

                if (!PerExperimentSettings.ContainsKey(nodeName))
                {
                    Log.Error("No experiment named '{0}' located.", nodeName);
                } else {
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
                              
                Save();
            }

#endregion
        }

        public void OnSave(ConfigNode node)
        {
            Log.Debug("Settings.save");

            #region general settings
                ConfigNode general = node.AddNode(new ConfigNode("GeneralSettings"));

                general.AddValue("SaveFlightSessionManeuverNodes", SaveFlightSessionManeuverNodes);
                general.AddValue("FlaskAnimationEnabled", FlaskAnimationEnabled);
                general.AddValue("SoundOnNewResearch", SoundOnNewResearch);

            #endregion

            #region experiment settings
            ConfigNode expSettings = node.AddNode(new ConfigNode("ExperimentSettings"));

            foreach (var kvp in PerExperimentSettings)
                kvp.Value.OnSave(expSettings.AddNode(new ConfigNode(kvp.Key)));
            
            #endregion
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

        public bool SaveFlightSessionManeuverNodes { get; set; }
        public bool FlaskAnimationEnabled { get; set; }
        public bool SoundOnNewResearch { get; set; }

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

    }


}
