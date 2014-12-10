﻿using System;
using System.Collections.Generic;
using ReeperCommon;

namespace ScienceAlert.ProfileData.Implementations
{
    /// <summary>
    /// A set of experiment settings for a particular vessel
    /// </summary>
    class Profile : IProfile
    {
        private float _scienceThreshold = 0f;
        private string _name;

        [NonSerialized]
        private Dictionary<string /* expid */, ProfileData.SensorSettings> _settings;


/******************************************************************************
 *                    Implementation Details
 ******************************************************************************/

        /// <summary>
        /// Creates a new profile, name and settings to be loaded from
        /// specified ConfigNode
        /// </summary>
        /// <param name="node"></param>
        public Profile(ConfigNode node)
        {
            Setup();

            // load from specified node
            OnLoad(node);

            RegisterEvents();
        }



        /// <summary>
        /// Creates a default profile of the specified name with default
        /// settings
        /// </summary>
        /// <param name="name"></param>
        public Profile(string name)
        {
            // default values
            Log.Verbose("Creating profile '{0}' with default values", name);
            this.Name = name;
            Setup();
            RegisterEvents();
        }



        /// <summary>
        /// Copy constructor
        /// </summary>
        /// <param name="other"></param>
        public Profile(Profile other)
        {
            var otherKeys = other._settings.Keys;

            _settings = new Dictionary<string, ProfileData.SensorSettings>();

            foreach (var otherKey in otherKeys)
                _settings.Add(otherKey, new ProfileData.SensorSettings(other._settings[otherKey]));

            Name = String.Copy(other.Name);
            Modified = other.Modified;
            _scienceThreshold = other._scienceThreshold;

            RegisterEvents();
        }



        /// <summary>
        /// Some necessary setup shared between constructors
        /// </summary>
        private void Setup()
        {
            _settings = new Dictionary<string, ProfileData.SensorSettings>();

            try
            {
                var expids = ResearchAndDevelopment.GetExperimentIDs();

                foreach (var id in expids)
                    _settings.Add(id, new ProfileData.SensorSettings());
            }
            catch (Exception e)
            {
                // this is most likely to happen on GetExperimentIDs, which
                // will throw an exception if there are duplicate experiment
                // id entries
                Log.Error("Profile '{1}' constructor exception: {0}", e, string.IsNullOrEmpty(Name) ? "(unnamed)" : Name);
            }
        }



        /// <summary>
        /// Saved into the node we're given
        /// </summary>
        /// <param name="node"></param>
        public void OnSave(ConfigNode node)
        {
            Log.Debug("Saving profile...");

            ConfigNode.CreateConfigFromObject(this, 0, node);

            foreach (var kvp in _settings)
                kvp.Value.OnSave(node.AddNode(new ConfigNode(kvp.Key)));

            Log.Debug("Profile: OnSave config: {0}", node.ToString());
        }



        /// <summary>
        /// Load from the node we're given
        /// </summary>
        /// <param name="node"></param>
        public void OnLoad(ConfigNode node)
        {
            Log.Debug("Loading profile...");
            ConfigNode.LoadObjectFromConfig(this, node);
            if (string.IsNullOrEmpty(Name))
            {
                // the saved profile somehow doesn't have a name. Since we
                // distinguish them this way, that's a pretty bad thing
                Name = "nameless." + Guid.NewGuid().ToString();
                Log.Warning("Profile.OnLoad: Loaded a profile without a name specified! It has been renamed to '{0}'", Name);
                
            } else Log.Debug("Profile name is '{0}'", Name);

            // it's possible that the ConfigNode we're loading from has
            // extra experiment ids in it. Try to preserve those, if possible
            foreach (var expid in node.nodes.DistinctNames())
            {
                var expNode = node.GetNode(expid);

                if (!_settings.ContainsKey(expid))
                {
                    Log.Verbose("Profile.OnLoad: Expid '{0}' not found in default set. Adding it.", expid);
                    _settings.Add(expid, new ProfileData.SensorSettings());
                }

                _settings[expid].OnLoad(expNode);
            }
        }



        public IProfile Clone()
        {
            return new Profile(this);
        }



        /// <summary>
        /// Just a convenience function to make it more obvious what's 
        /// happening when a completely default profile is needed
        /// </summary>
        /// <returns></returns>
        public static Profile MakeDefault()
        {
            return new Profile("default");
        }



        /// <summary>
        /// A shortcut to get experiment settings
        /// </summary>
        /// <param name="expid"></param>
        /// <returns></returns>
        public ProfileData.SensorSettings GetSensorSettings(string expid)
        {
            if (_settings.ContainsKey(expid))
                return _settings[expid];

            // I never expect to see this. If it shows up, something in
            // loading or initialization has broken
            Log.Warning("Profile '{0}' has no settings for expid {1}; creating a default", Name, expid);

            _settings[expid] = new ProfileData.SensorSettings();

            return _settings[expid];
        }


        public void SetSensorSettings(string expid, SensorSettings settings)
        {
            if (_settings.ContainsKey(expid))
                _settings[expid] = settings;
            else _settings.Add(expid.ToLower(), settings);
        }


        public string Name {
            get
            {
                return _name;
            }

            set
            {
                if (!string.Equals(_name, value))
                    Modified = true;
                _name = value;
            } 
        }


        public string DisplayName
        {
            get
            {
                if (Modified)
                    return "*" + Name + "*";
                return Name;
            }
        }


        public float ScienceThreshold
        {
            get
            {
                return _scienceThreshold;
            }
            set
            {
                if (Math.Abs(value - _scienceThreshold) > 0.001f)
                    Modified = true;
                _scienceThreshold = value;
            }
        }


        public bool Modified { get; set; }


        void SettingChanged()
        {
            Log.Debug("Profile '{0}' was modified!", Name);
            Modified = true;
        }


        /// <summary>
        /// Registers this Profile for all of its owned experiment changed events
        /// </summary>
        private void RegisterEvents()
        {
            foreach (var kvp in _settings)
                kvp.Value.OnChanged += SettingChanged;
        }
    }
}