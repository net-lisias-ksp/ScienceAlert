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
    /// A set of experiment settings for a particular vessel
    /// </summary>
    class Profile
    {
        [Persistent (isPersistant = true)]
        public string name = string.Empty;

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
            Setup();
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

                Log.Verbose("Profile '{0}' created successfully", name);
            }
            catch (Exception e)
            {
                // this is most likely to happen on GetExperimentIDs, which
                // will throw an exception if there are duplicate experiment
                // id entries
                Log.Error("Profile '{1}' constructor exception: {0}", e, name);
            }
        }



        public void OnSave(ConfigNode node)
        {
            Log.Debug("Saving profile...");

            Log.Debug("Created config: {0}", ConfigNode.CreateConfigFromObject(this, 0, node).ToString());

            ConfigNode.CreateConfigFromObject(this, 0, node);

            foreach (var kvp in settings)
            {
                kvp.Value.OnSave(node.AddNode(new ConfigNode(kvp.Key)));
            }

            Log.Debug("Profile: OnSave config: {0}", node.ToString());
#warning confirm this is right
        }



        public void OnLoad(ConfigNode node)
        {
            Log.Debug("Loading profile...");
            ConfigNode.LoadObjectFromConfig(this, node);
            Log.Debug("Profile name is '{0}'", name);

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


        /// <summary>
        /// merely a convenience property. Profile is just dumb data;
        /// its owner ProfileStorage is where the current profile is
        /// tracked
        /// </summary>
        public static Profile Current
        {
            get
            {
                if (ProfileStorage.Instance == null) return null;
                return ProfileStorage.CurrentProfile;
            }
            set
            {
                if (ProfileStorage.Instance == null)
                {
                    Log.Error("Profile: ProfileStorage is null; cannot set current profile");
                }
                else ProfileStorage.CurrentProfile = value;
            }
        }
    }
}
