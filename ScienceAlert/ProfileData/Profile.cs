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
using UnityEngine;
using ReeperCommon;

namespace ScienceAlert.ProfileData
{
    /// <summary>
    /// A set of experiment settings for a particular vessel
    /// </summary>
    class Profile
    {
        //------------------------------------------------------
        // Profile identifier
        //------------------------------------------------------
        [Persistent (isPersistant = true)]
        public string name = string.Empty;

        [Persistent]
        public bool modified = false;


        //------------------------------------------------------
        // Science threshold
        //------------------------------------------------------

        [Persistent]
        public float scienceThreshold = 0f;


        //------------------------------------------------------
        // Per-experiment settings
        //------------------------------------------------------
        [NonSerialized]
        public Dictionary<string /* expid */, ProfileData.ExperimentSettings> settings;


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
            this.name = name;
            Setup();
            RegisterEvents();
        }



        /// <summary>
        /// Copy constructor
        /// </summary>
        /// <param name="other"></param>
        public Profile(Profile other)
        {
            var otherKeys = other.settings.Keys;

            settings = new Dictionary<string, ProfileData.ExperimentSettings>();

            foreach (var otherKey in otherKeys)
                settings.Add(otherKey, new ProfileData.ExperimentSettings(other.settings[otherKey]));

            name = String.Copy(other.name);
            modified = other.modified;
            scienceThreshold = other.scienceThreshold;

            RegisterEvents();
        }



        /// <summary>
        /// Some necessary setup shared between constructors
        /// </summary>
        private void Setup()
        {
            settings = new Dictionary<string, ProfileData.ExperimentSettings>();

            try
            {
                var expids = ResearchAndDevelopment.GetExperimentIDs();

                foreach (var id in expids)
                    settings.Add(id, new ProfileData.ExperimentSettings());
            }
            catch (Exception e)
            {
                // this is most likely to happen on GetExperimentIDs, which
                // will throw an exception if there are duplicate experiment
                // id entries
                Log.Error("Profile '{1}' constructor exception: {0}", e, string.IsNullOrEmpty(name) ? "(unnamed)" : name);
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

            foreach (var kvp in settings)
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
            if (string.IsNullOrEmpty(name))
            {
                // the saved profile somehow doesn't have a name. Since we
                // distinguish them this way, that's a pretty bad thing
                name = "nameless." + Guid.NewGuid().ToString();
                Log.Warning("Profile.OnLoad: Loaded a profile without a name specified! It has been renamed to '{0}'", name);
                
            } else Log.Debug("Profile name is '{0}'", name);

            // it's possible that the ConfigNode we're loading from has
            // extra experiment ids in it. Try to preserve those, if possible
            foreach (var expid in node.nodes.DistinctNames())
            {
                var expNode = node.GetNode(expid);

                if (!settings.ContainsKey(expid))
                {
                    Log.Verbose("Profile.OnLoad: Expid '{0}' not found in default set. Adding it.", expid);
                    settings.Add(expid, new ProfileData.ExperimentSettings());
                }

                settings[expid].OnLoad(expNode);
            }
        }



        public Profile Clone()
        {
            Profile p = new Profile(this);
            return p;
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
        public ProfileData.ExperimentSettings this[string expid]
        {
            get
            {
                if (settings.ContainsKey(expid))
                    return settings[expid];

                // I never expect to see this. If it shows up, something in
                // loading or initialization has broken
                Log.Warning("Profile '{0}' has no settings for expid {1}; creating a default", name, expid);

                settings[expid] = new ProfileData.ExperimentSettings();

                return settings[expid];
            }

            private set
            {
                settings.Add(expid.ToLower(), value);
            }
        }



        public string DisplayName
        {
            get
            {
                if (modified)
                    return "*" + name + "*";
                return name;
            }
        }



        public float ScienceThreshold
        {
            get
            {
                return scienceThreshold;
            }
            set
            {
                if (value != scienceThreshold)
                    modified = true;
                scienceThreshold = value;
            }
        }



        void SettingChanged()
        {
            Log.Debug("Profile '{0}' was modified!", name);
            modified = true;
        }


        /// <summary>
        /// Registers this Profile for all of its owned experiment changed events
        /// </summary>
        private void RegisterEvents()
        {
            foreach (var kvp in settings)
                kvp.Value.OnChanged += SettingChanged;
        }
    }
}
