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
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using ReeperCommon;

namespace ScienceAlert
{
    /// <summary>
    /// All _saved_ user-created profiles are stored by this
    /// object in a config file in the ScienceAlert directory so that
    /// they can be shared across different game sessions.
    /// 
    /// This object is also used to track the current profile in use.
    /// </summary>
    class ProfileStorage : MonoBehaviour
    {
        Dictionary<string /* profile name */, Profile> profiles = new Dictionary<string, Profile>();
        Profile current;

/******************************************************************************
 *                    Implementation Details
 ******************************************************************************/

        /// <summary>
        /// Called when script is created. Load all saved profiles from disk.
        /// </summary>
        void Awake()
        {
            Load();
            
            // make sure there's a default entry available
            if (!profiles.Keys.Any(name => name.ToLower().Equals("default")))
                profiles.Add("default", Profile.MakeDefault());

            Log.Normal("ProfileStorage: loaded {0} profiles.", profiles.Count);
            foreach (var name in profiles.Keys)
                Log.Debug("  Profile: {0}", name);

            Instance = this;
        }


        void Start()
        {
            // register for events
            GameEvents.onVesselChange.Add(OnVesselChange);

            OnVesselChange(FlightGlobals.ActiveVessel);
        }


        public void OnVesselChange(Vessel v)
        {
            // ensure this vessel has a ModuleSAProfile on it. The module
            // will handle things from there
            var modules = v.FindPartModulesImplementing<ProfileData.ModuleSAProfile>();

            if (modules.Count == 0)
            {
                Log.Warning("Vessel '{0}' does not have a ScienceAlert profile module. Adding ...", v.vesselName);

                var pm = v.rootPart.AddModule("ModuleSAProfile");
                pm.SendMessage("Awake");
                pm.SendMessage("Start");

                var sap = pm as ProfileData.ModuleSAProfile;
                
                // since this is a brand new profile, we'll give it the saved default
                sap.modified = false;
                sap.profileName = GetDefault().name;
                sap.Profile = GetDefault(); // note to self: module will clone this itself
            }
        }



        /// <summary>
        /// Called when the GameObject (ScienceAlert) this script is attached
        /// to is destroyed. Saves profile data to ScienceAlert directory.
        /// </summary>
        void OnDestroy()
        {
            Log.Debug("ProfileStorage: being destroyed");

            // unregister for events
            GameEvents.onVesselChange.Remove(OnVesselChange);

            Instance = null;
            Save();
        }



        /// <summary>
        /// Load profiles from disk
        /// </summary>
        public void Load()
        {
            try
            {
                // locate profile config
                var path = ConfigUtil.GetDllDirectoryPath() + "/profiles.cfg";

                if (File.Exists(path))
                {
                    Log.Normal("Loading ConfigNode from '{0}'", path);
                    ConfigNode existing = ConfigNode.Load(path);
                    OnLoad(existing);
                }
                else
                {
                    Log.Warning("Did not find '{0}', creating default");
                    profiles.Add("default", Profile.MakeDefault());
                    Save();
                    Load();
                }
            }
            catch (Exception e)
            {
                Log.Error("ProfileStorage: Exception in Start: {0}", e);
            }
        }



        /// <summary>
        /// Save profiles to disk
        /// </summary>
        public void Save()
        {
            var path = ConfigUtil.GetDllDirectoryPath() + "/profiles.cfg";
            Log.Verbose("Saving profiles to '{0}'", path);

            try
            {
                ConfigNode node = new ConfigNode("SCIENCEALERT_PROFILES");
                OnSave(node);

                Log.Verbose("Saving to disk");
                File.WriteAllText(path, node.ToString());
            } catch (Exception e)
            {
                Log.Error("Error while saving profiles; some or all profiles may have been lost: {0}", e);
            }
        }



        /// <summary>
        /// Load all profiles from given ConfigNode. Ensure that a "default"
        /// exists
        /// </summary>
        /// <param name="node"></param>
        private void OnLoad(ConfigNode node)
        {
            Log.Debug("ProfileStorage: loading saved profiles from: {0}", node.ToString());

            foreach (ConfigNode profileNode in node.nodes)
            {
                Log.Verbose("ProfileStorage: loading node {0}", profileNode.name);
                Log.Debug("{0} contents: {1}", profileNode.name, profileNode.ToString());

                try // we really can't afford to let any exception screw this up or else
                    // the player's carefully configured profiles will be lost on the next
                    // save
                {
                    // make sure we don't have any duplicates; that'd be bad
                    if (profiles.ContainsKey(profileNode.name))
                    {
                        Log.Warning("Profile '{0}' has a duplicate entry; discarding", profileNode.name);
                        continue;
                    }
                    else
                    {
                        profiles.Add(profileNode.name, new Profile(profileNode));
                        Log.Verbose("ProfileStorage: loaded profile {0}", profileNode.name);
                    }
                } catch (Exception e)
                {
                    Log.Error("ProfileStorage: Exception while loading profiles: {0}", e);
                }
            }
        }



        /// <summary>
        /// Save all known profiles
        /// </summary>
        /// <param name="node"></param>
        private void OnSave(ConfigNode node)
        {
            // node should be "SCIENCEALERT_PROFILES"

            node = node.AddNode(new ConfigNode("SCIENCEALERT_PROFILES"));

            foreach (var kvp in profiles)
            {
                Log.Debug("ProfileStorage: saving '{0}'", kvp.Key);

                var profileNode = node.AddNode(KSPUtil.SanitizeString(kvp.Key, '_', true));
                kvp.Value.OnSave(profileNode);
            }
        }


        public Profile GetProfile(string name)
        {
            if (!profiles.ContainsKey(name))
            {
                Log.Debug("ProfileStorage.GetProfile: '{0}' was requested but does not exist", name);
                return null;
            }
            else return profiles[name];
        }



        /// <summary>
        /// Returns the user-defined default profile
        /// </summary>
        /// <returns></returns>
        public Profile GetDefault()
        {
            var key = profiles.Keys.First(k => k.ToLower().Equals("default"));
            if (key == null)
            {
                Log.Error("ProfileStorage: somehow doesn't contain a default entry (?!)");
                return Profile.MakeDefault();
            }
            else return profiles[key];
        }



        /// <summary>
        /// All interaction with a particular profile goes through this
        /// </summary>
        public static ProfileStorage Instance
        {
            get;
            private set;
        }


        public static Profile CurrentProfile
        {
            get
            {
                var storage = ProfileStorage.Instance;

                if (storage == null) return null;

                return storage.current;
            }
        }
    }
}
