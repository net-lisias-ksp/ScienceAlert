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
using ReeperCommon;
using UnityEngine;

namespace ScienceAlert.ProfileData
{
    /// <summary>
    /// This module 
    /// </summary>
    class ModuleSAProfile : PartModule
    {
        [Persistent]
        public string profileName = "default";

        [Persistent]
        public bool modified = false;

        private Profile profile;

/******************************************************************************
 *                    Implementation Details
 ******************************************************************************/
        void Start()
        {
            profile = ProfileStorage.Instance.GetDefault().Clone();

            // register for events
            GameEvents.onVesselWasModified.Add(OnVesselModified);
            GameEvents.onSameVesselDock.Add(OnSameVesselDock);
            GameEvents.onSameVesselUndock.Add(OnSameVesselUndock);
            GameEvents.onPartDestroyed.Add(OnPartDestroyed);
        }



        void OnDestroy()
        {
            // unregister events
            GameEvents.onVesselWasModified.Remove(OnVesselModified);
            GameEvents.onSameVesselDock.Remove(OnSameVesselDock);
            GameEvents.onSameVesselUndock.Remove(OnSameVesselUndock);
            GameEvents.onPartDestroyed.Remove(OnPartDestroyed);
        }



        public void OnPartDestroyed(Part part)
        {
            if (part.vessel != this.part.vessel) return;

            Log.Debug("SAProfile: OnPartDestroyed vessel {0}, part {1}", part.vessel.vesselName, part.ConstructID);
        }



        public void OnVesselModified(Vessel vessel)
        {
            if (part.vessel != vessel) return;

            // check to see if we're still the root part. Destroy any
            // extra ModuleSAProfiles not on the root part
        }



        public void OnSameVesselDock(GameEvents.FromToAction<ModuleDockingNode, ModuleDockingNode> nodes)
        {
            Log.Debug("OnSameVesselDock: from {0} to {1}", nodes.from.part.ConstructID, nodes.to.part.ConstructID);

            // same logic as modified
        }



        public void OnSameVesselUndock(GameEvents.FromToAction<ModuleDockingNode, ModuleDockingNode> nodes)
        {
            Log.Debug("OnSameVesselUndock: from {0} to {1}", nodes.from.part.ConstructID, nodes.to.part.ConstructID);
        }




        public override void OnLoad(ConfigNode node)
        {
            Log.Debug("ModuleSAProfile.OnLoad with {0}", node.ToString());

            base.OnLoad(node);

            if (modified)
            {
                Log.Verbose("Part {0} contains a modified profile called '{1}'; loading it", part.ConstructID, profile.name);

                profile = new Profile(node);
            } else
            {
                Log.Verbose("ModuleSAProfile contains unmodified profile; retrieving...");

                // alright, get a profile out of storage. We actually
                // want a clone here so the profile held by ModuleSAProfile
                // can be tweaked without affecting any stored profiles
                // until the player specifically tells us we should overwrite
                // one

                var realProfile = ProfileStorage.Instance.GetProfile(name);
                if (realProfile == null)
                {
                    Log.Error("ModuleSAProfile: Failed to find a profile called '{0}'; using default", profileName);
                    profile = ProfileStorage.Instance.GetDefault().Clone();
                }
                else
                {
                    Log.Debug("ModuleSAProfile: Cloning profile");
                    profile = realProfile.Clone();
                }
            }
        }



        public override void OnSave(ConfigNode node)
        {
            base.OnSave(node);

            if (modified)
            {
                Log.Verbose("Part {0} contains a modified profile called '{1}'; saving it", part.ConstructID, profile.name);

                if (profile == null)
                {
                    Log.Error("ModuleSAProfile: modified flag is set but no profile exists!");
                }
                else
                {
                    profile.OnSave(node);
                }
                
            } // else do nothing; storage has it
        }



        /// <summary>
        /// We always want a given module to have essentially its own copy
        /// of a profile so that temporary adjustments to it don't affect
        /// any saved profiles, so we'll make sure we don't have any references
        /// to profiles we're assigned
        /// </summary>
        public Profile Profile
        {
            get
            {
                return profile;
            }
            set
            {
                // we ALWAYS want our own copy, so make sure it's a clone
                profile = value.Clone();
                profileName = profile.name;

                Log.Debug("ModuleSAProfile on vessel {0} on part {1} cloned a profile called {2}", vessel.vesselName, part.ConstructID, profileName);
            }
        }
    }
}
