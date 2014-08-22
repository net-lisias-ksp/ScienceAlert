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
using ScienceAlert.ProfileData;

namespace ScienceAlert
{
    using ProfileTable = Dictionary<string, Profile>;   
    using VesselTable = Dictionary<Guid, Profile>;     

    /// <summary>
    /// I thought long and hard and decided on a ScenarioModule for this logic.
    /// 
    /// I considered adding a PartModule on every ship's root part, but it's
    /// messy to keep track of and there have been some reports of added modules
    /// mysteriously vanishing.
    /// 
    /// I then considered something very similar to this object but as a 
    /// regular KSPAddon. This ultimately was rejected because of the potential
    /// to lose data (vessel-specific profiles) due to quicksaves. One solution 
    /// was to never delete any profile that ever existed ... but when you
    /// consider all the ways that "vessels" can be created (debris, asteroids,
    /// and so on) then no matter how you slice it nor how good you filter out
    /// non-interesting vessels, you still end up in a situation where file size
    /// could snowball sooner or later.
    /// 
    /// I wanted to avoid having two separate ConfigNodes storing profiles but
    /// it's ultimately the cleanest way to do this. Here's how things are
    /// organized:
    /// 
    /// "Stored Profiles" - These are profiles the user specifically asked us
    ///                     to save. One example would be a profile that always
    ///                     alerts if any science whatsoever available on 
    ///                     transmitted data.
    ///                     
    /// "Vessel Profiles" - These are very similar to stored profiles, except 
    ///                     that every vessel has its own clone it can modify.
    ///                     
    ///                     They also get a "modified" flag. If this flag is
    ///                     false, a stored profile of the same name will be
    ///                     used instead of the vessel-stored version. If this
    ///                     profile were to change in any way, however -- any
    ///                     option changed -- then its "modified" flag is set
    ///                     to true and it is essentially disconnected from
    ///                     the stored profile. 
    ///                     
    ///                     When this vessel's profile is loaded from the 
    ///                     persistence file and that flag is set, then 
    ///                     the vessel's cloned (and now modified) version 
    ///                     of the profile is used instead of any stored 
    ///                     profile.
    ///                     
    ///                     And finally, should the vessel's profile name not
    ///                     be found in the "stored" profiles on load (because 
    ///                     it was deleted by the user previously), then the 
    ///                     vessel will receive the vessel-specific profile
    ///                     instead of a stored profile
    /// </summary>
    //[KSPScenario(ScenarioCreationOptions.AddToNewCareerGames            |
    //             ScenarioCreationOptions.AddToExistingCareerGames       |
    //             ScenarioCreationOptions.AddToNewScienceSandboxGames    |
    //             ScenarioCreationOptions.AddToExistingScienceSandboxGames,

    //             GameScenes.FLIGHT)] // note to self: this apparently only triggers on a main menu -> game transition;
    //                                 // debug scene jumping code will break it
    class ProfileManager : MonoBehaviour
    {
        private readonly string ProfileStoragePath = ConfigUtil.GetDllDirectoryPath() + "/profiles.cfg";
        ProfileTable storedProfiles;
        VesselTable vesselProfiles;    


/******************************************************************************
 *                    Implementation Details
 ******************************************************************************/

#region intialization/deinitialization



        /// <summary>
        /// Load all saved profiles, register for events and other
        /// initialization tasks
        /// </summary>
        void Start()
        {
            Log.Debug("ProfileManager.Start");

            if (HighLogic.CurrentGame.config == null)
            {
                Log.Error("CurrentGame.config == null!");
                HighLogic.CurrentGame.config = new ConfigNode();
            }

            Settings.Instance.OnSave += OnSettingsSave;

            GameEvents.onVesselChange.Add(OnVesselChange);
            GameEvents.onVesselDestroy.Add(OnVesselDestroy);
            GameEvents.onVesselCreate.Add(OnVesselCreate);
            GameEvents.onVesselWasModified.Add(OnVesselModified);
            GameEvents.onFlightReady.Add(OnFlightReady);
            GameEvents.onVesselWillDestroy.Add(OnVesselWillDestroy);
            GameEvents.onGameStateSave.Add(OnGameSave);
            //GameEvents.onGameStateLoad.Add(OnGameLoad);


            LoadStoredProfiles();
            OnGameLoad(HighLogic.CurrentGame.config);
        }



        /// <summary>
        /// Unregister for any events from the constructor; save 
        /// stored profiles
        /// </summary>
        void OnDestroy()
        {
            Log.Debug("ProfileManager: OnDestroy");

            GameEvents.onVesselChange.Remove(OnVesselChange);
            GameEvents.onVesselDestroy.Remove(OnVesselDestroy);
            GameEvents.onVesselCreate.Remove(OnVesselCreate);
            GameEvents.onVesselWasModified.Remove(OnVesselModified);
            GameEvents.onFlightReady.Remove(OnFlightReady);
            GameEvents.onVesselWillDestroy.Remove(OnVesselWillDestroy);
            GameEvents.onGameStateSave.Remove(OnGameSave);
            //GameEvents.onGameStateLoad.Remove(OnGameLoad);

            SaveStoredProfiles();
        }


        
        /// <summary>
        /// Load stored profiles from a ConfigNode in the ScienceAlert
        /// directory
        /// </summary>
        private void LoadStoredProfiles()
        {
            try
            {
                storedProfiles = new ProfileTable();

                if (!System.IO.File.Exists(ProfileStoragePath))
                {
                    Log.Warning("ProfileManager: Profile config not found at '{0}'", ProfileStoragePath);
                } else {
                    Log.Debug("ProfileManager: Loading profile config from '{0}'", ProfileStoragePath);

                    ConfigNode stored = ConfigNode.Load(ProfileStoragePath);

                    if (stored == null || !stored.HasNode("SCIENCEALERT_PROFILES"))
                    {
                        Log.Error("ProfileManager: Failed to load config");
                    }
                    else
                    {
                        stored = stored.GetNode("SCIENCEALERT_PROFILES"); // to avoid having an empty cfg, which will
                                                                          // cause KSP to hang at load

                        var profiles = stored.GetNodes("PROFILE");
                        Log.Verbose("Found {0} stored profiles to load", profiles.Length);

                        foreach (var profileNode in profiles)
                        {
                            try
                            {
                                Profile p = new Profile(profileNode);
                                p.modified = false; // by definition, stored profiles haven't been modified

                                storedProfiles.Add(p.name, p);
                                Log.Verbose("Loaded profile '{0}' successfully!", p.name);
                            } 
                            catch (Exception e) 
                            {
                                Log.Error("ProfileManager: profile '{0}' failed to parse; {1}", name, e);
                            }
                        }
                    }
                }
               
                // make sure there's a "default" config in there. Ideally the
                // user has created and saved over one but if not, we need
                // at least a default to give to vessels that are missing their
                // profiles
                if (DefaultProfile == null)
                    storedProfiles.Add("default", Profile.MakeDefault());

            } catch (Exception e)
            {
                Log.Error("ProfileManager: Exception loading stored profiles: {0}", e);

                // don't keep anything that might have been loaded; something's
                // gone seriously wrong but we might manage to salvage things if
                // we accept the loss of stored data and use the vessel-specific
                // profiles instead
                storedProfiles = new ProfileTable();
            }
        }



        /// <summary>
        /// Stored profiles go into a cfg ConfigNode in the ScienceAlert
        /// directory
        /// 
        /// Precondition: All profile names have been sanitized
        /// </summary>
        private void SaveStoredProfiles()
        {
            ConfigNode profiles = new ConfigNode("SCIENCEALERT_PROFILES"); // note: gave it a name because an empty
                                                                     // ConfigNode will cause KSP to choke on load

            foreach (var kvp in storedProfiles)
            {
                try
                {
                    // if this happened, something broke when we were creating
                    // a profile (or potentially loading an unsanitized one)
                    if (!kvp.Key.ToLower().Equals(kvp.Value.name.ToLower()))
                        Log.Warning("ProfileManager.SavedStoredProfiles: stored key '{0}' does not match profile name '{1}'!", kvp.Key, kvp.Value.name);

                    Log.Verbose("Saving stored profile '{0}'", kvp.Key);
                    kvp.Value.OnSave(profiles.AddNode(new ConfigNode("PROFILE")));
                    Log.Verbose("Saved '{0}'", kvp.Value.name);
                } catch (Exception e)
                {
                    Log.Error("ProfileManager: Exception while saving '{0}': {1}", kvp.Key, e);
                }
            }

#if DEBUG
            Log.Debug("ProfileManager: stored profile ConfigNode: {0}", profiles.ToString());
#endif

            // note: removed because ConfigNode.Save seems to strip out
            //       the root node of the ConfigNode. That's bad because if there
            //       aren't any profiles saved (due to a fail somewhere) in the root
            //       node, the player's game will freeze at load when it encounters
            //       the empty cfg
            //if (!profiles.Save(ProfileStoragePath, "ScienceAlert stored profiles"))
                //Log.Error("ProfileManager: Error while saving stored profiles to '{0}'! Any changes this session have been lost!", ProfileStoragePath);

            System.IO.File.WriteAllText(ProfileStoragePath, profiles.ToString());
        } 



#endregion

#region GameEvents

        private void OnVesselChange(Vessel vessel)
        {
            Log.Debug("OnVesselChange: {0}", vessel.vesselName);
        }



        private void OnVesselDestroy(Vessel vessel)
        {
            Log.Debug("OnVesselDestroy: {0}", vessel.vesselName);
        }



        private void OnVesselCreate(Vessel newVessel)
        {
            Log.Debug("OnVesselCreate: {0}", newVessel.vesselName);  
        }



        private void OnVesselModified(Vessel vessel)
        {
            Log.Debug("OnVesselModified: {0}", vessel.vesselName);   
        }



        private void OnFlightReady()
        {
            Log.Debug("OnFlightReady");
        }


        private void OnVesselWillDestroy(Vessel vessel)
        {
            Log.Debug("OnVesselWillDestroy: {0}", vessel.vesselName); 
        }



        /// <summary>
        /// Load vessel-specific ConfigNodes from the persistent file
        /// </summary>
        /// <param name="node"></param>
        private void OnGameLoad(ConfigNode node)
        {
            Log.Verbose("ProfileManager.OnGameLoad = {0}", node.ToString());

            if (node == null) Log.Error("node is null!");
            if (!node.HasNode("SCIENCEALERT_PROFILES"))
            {
                Log.Warning("Persistent save has no saved profiles");
                vesselProfiles = new VesselTable();
                return;
            }
            else node = node.GetNode("SCIENCEALERT_PROFILES");

            List<string> errors = new List<string>();
            vesselProfiles = new VesselTable();

            var guidStrings = node.nodes.DistinctNames();
            Log.Verbose("ProfileManager: {0} vessel profile nodes found", guidStrings.Length);

            foreach (var strGuid in guidStrings)
            {
                Log.Debug("Loading node with name '{0}'", strGuid);

                try
                {
                    Guid guid = new Guid(strGuid);  // could throw an exception if string is malformed
                    Log.Debug("Guid created: {0}", guid.ToString());

                    // confirm a vessel with this Guid exists
                    if (!FlightGlobals.Vessels.Any(v => v.id == guid))
                    {
                        Log.Warning("Did not find a vessel that matches {0}; check destruction event code", guid.ToString());
                        continue;
                    }

                    // confirm that we don't have duplicate entries
                    if (vesselProfiles.ContainsKey(guid))
                    {
                        Log.Error("ProfileManager: Duplicate profile for vessel {0} found!", VesselIdentifier(guid), FlightGlobals.Vessels.Find(v => v.id == guid).vesselName);
                        continue;
                    }

                    // grab the node with this info
                    ConfigNode profileNode = node.GetNode(strGuid);

                    // create a profile out of the data stored in this node
                    Profile p = new Profile(profileNode);

                    // if modified is true => use the modified profile
                    // if modified is false THEN
                    //      if a stored profile of same name exists THEN 
                    //          clone the stored profile
                    //      Else
                    //          add to missing profile list
                    //          clone default profile
                    //      end
                    // end
                    if (p.modified)
                    {
                        Log.Verbose("Vessel {0} has a modified profile '{1}' stored.", VesselIdentifier(guid), p.name);

                        vesselProfiles.Add(guid, p);
                    }
                    else
                    {
                        if (HaveStoredProfile(p.name))
                        {
                            Log.Verbose("Vessel {0} has stored profile '{1}'", VesselIdentifier(guid), p.name);

                            // use the stored profile
                            vesselProfiles.Add(guid, FindStoredProfile(p.name).Clone());

                        }
                        else
                        {
                            // add to missing profile list and clone default
                            errors.Add(string.Format("Stored profile '{0}' not found for {1}", p.name, VesselIdentifier(guid)));
                            Log.Error("Could not find profile '{0}' for vessel {1}. Will use default.", p.name, VesselIdentifier(guid));

                            // note to self: this isn't the same as Profile.MakeDefault();
                            // that is used for a truly default, totally unmodified profile.
                            // DefaultProfile will locate a profile called "default" instead,
                            // which may be custom-made by the user if they overwrite the 
                            // standard one
                            vesselProfiles.Add(guid, DefaultProfile.Clone());

                        }
                    }
                }
                catch (Exception e)
                {
                    Log.Error("ProfileManager: Exception while loading '{0}': {1}", strGuid, e);
                }
            }

            if (errors.Count > 0)
            {
                string message = "Errors while loading profiles:\n\n";

                errors.ForEach(err => message += err + "\n");
                message += "\nVessel(s) have been assigned the \"default\" profile.";

                Log.Debug("Errors encountered during profile load: {0}", message);

                PopupDialog.SpawnPopupDialog("ScienceAlert: Profile Manager", message, "Okay", false, HighLogic.Skin);
            }
        }



        /// <summary>
        /// Save vessel-specific profiles to the persistent ConfigNode
        /// </summary>
        /// <param name="node"></param>
        private void OnGameSave(ConfigNode node)
        {
            Log.Verbose("ProfileManager.OnGameSave: {0}", node.ToString());

            if (!node.HasNode("SCIENCEALERT_PROFILES")) node.AddNode("SCIENCEALERT_PROFILES");

            node = node.GetNode("SCIENCEALERT_PROFILES");

            Log.Verbose("ProfileManager.OnSave: Saving {0} vessel profiles", vesselProfiles.Count);

            foreach (var kvp in vesselProfiles)
            {
                try
                {
                    if (!FlightGlobals.Vessels.Any(v => v.id == kvp.Key))
                    {
                        Log.Normal("ProfileManager.OnSave: Not saving profile '{0}' because vessel {1} does not exist.", kvp.Value.name, kvp.Key.ToString());
                        continue;
                    }
                    else
                    {
                        Log.Verbose("ProfileManager.OnSave: saving vessel profile '{0}'", kvp.Value.name);
                        kvp.Value.OnSave(node.AddNode(new ConfigNode(kvp.Key.ToString())));
                    }
                }
                catch (Exception e)
                {
                    Log.Error("ProfileManager.OnSave: Exception while saving profile '{0}': {1}", string.Format("{0}:{1}", kvp.Key.ToString(), kvp.Value.name), e);
                    continue;
                }
            }

            Log.Verbose("ProfileManager.OnGameSave: Finished");
        }

#endregion

#region other events

        /// <summary>
        /// Called when Settings.Save is called
        /// </summary>
        /// <param name="node"></param>
        public void OnSettingsSave(ConfigNode node)
        {
            Log.Debug("ProfileManager.OnSettingsSave");

            SaveStoredProfiles();
        }

#endregion

#region Interaction methods


        public Profile DefaultProfile
        {
            get
            {
                var key = storedProfiles.Keys.SingleOrDefault(k => k.ToLower().Equals("default"));

                if (string.IsNullOrEmpty(key))
                {
                    Log.Error("ProfileManager.DefaultProfile: failed to find a default profile! Creating one.");
                    key = "default";
                    storedProfiles.Add(key, Profile.MakeDefault());
                }

                return storedProfiles[key];
            }
        }



        public Profile ActiveProfile
        {
            get
            {
                var vessel = FlightGlobals.ActiveVessel;

                if (vessel == null) 
                {
                    Log.Debug("WARN: ProfileManager.ActiveProfile: vessel is null");
                    return null;
                }
                if (!vesselProfiles.ContainsKey(vessel.id))
                {
                    Log.Normal("Vessel {0} does not have a vessel profile entry. Using default.", VesselIdentifier(vessel.id));
                    vesselProfiles.Add(vessel.id, DefaultProfile.Clone());
                }

                return vesselProfiles[vessel.id];
            }
        }




        public bool HasActiveProfile
        {
            get
            {
                return FlightGlobals.ActiveVessel != null;
            }
        }



        public int Count
        {
            get
            {
                if (storedProfiles != null)
                    return storedProfiles.Count;
                return 0;
            }
        }



        public ProfileTable.KeyCollection Names
        {
            get
            {
                return storedProfiles.Keys;
            }
        }

        public Profile GetProfileByName(string name)
        {
            var p = FindStoredProfile(name);
            if (p == null)
                Log.Error("Failed to find profile with key '{0}'", name);

            return p;
        }

        public ProfileTable Profiles
        {
            get
            {
                return storedProfiles;
            }
        }


        /// <summary>
        /// Adds the specified profile as an unmodified, stored profile.
        ///   Important note: does NOT save to disk
        /// </summary>
        /// <param name="profile"></param>
        /// <returns></returns>
        public void StoreActiveProfile(string name)
        {
            Profile p = ActiveProfile;

            p.name = name;
            p.modified = false;

            Profile newProfile = p.Clone();

            Log.Verbose("Adding new profile '{0}'..", p.name);

            var existing = FindStoredProfile(newProfile.name);
            if (existing != null) { Log.Warning("Overwriting existing profile"); storedProfiles.Remove(existing.name); }

            storedProfiles.Add(name, newProfile);
            Log.Verbose("Successfully added or updated profile");
        }



#endregion

#region internal methods

        private Profile FindStoredProfile(string name)
        {
            var key = storedProfiles.Keys.SingleOrDefault(k => k.ToLower().Equals(name.ToLower()));

            if (string.IsNullOrEmpty(key))
                return null;
            return storedProfiles[key];
        }

        public bool HaveStoredProfile(string name)
        {
            return FindStoredProfile(name) != null;
        }

        private string FindVesselName(Guid guid)
        {
            Vessel vessel = FlightGlobals.Vessels.SingleOrDefault(v => v.id == guid);
            if (vessel == null) return string.Format("<vessel {0} not found>", guid.ToString());
            return vessel.vesselName;
        }
      
        private string VesselIdentifier(Guid guid)
        {
            return string.Format("{0}:{1}", guid.ToString(), FindVesselName(guid));
        }
#endregion
    }
}
