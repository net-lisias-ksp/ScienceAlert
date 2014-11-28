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

namespace ScienceAlert.Experiments.Data
{
    using StorageList = List<IScienceDataContainer>;


    /// <summary>
    /// The purpose of this object is to reduce redundancy between 
    /// ExperimentObservers.  ScienceData isn't sorted by ScienceExperiment
    /// but rather by id, so it makes more sense to have one object track
    /// it than have every observer do so.
    /// 
    /// ScienceDataCache keeps track of events and will update itself as
    /// necessary.  It will also manage the magic transmitter as required.
    /// </summary>
    public class ScienceDataCache : MonoBehaviour
    {
        protected StorageList storage;                      // containers for science data
        protected MagicDataTransmitter magicTransmitter;    // MagicDataTransmitter keeps an eye on any queued data for the vessel
        protected Vessel vessel;                            // which vessel this storage cache is for


/******************************************************************************
 *                          Begin Implementation
 *****************************************************************************/
        public void Start()
        {
            GameEvents.onVesselChange.Add(OnVesselChange);
            GameEvents.onVesselWasModified.Add(OnVesselModified);
            GameEvents.onVesselDestroy.Add(OnVesselDestroyed);

            vessel = FlightGlobals.ActiveVessel;
            ScheduleRebuild();
        }



        public void OnDestroy()
        {
            GameEvents.onVesselDestroy.Remove(OnVesselDestroyed);
            GameEvents.onVesselWasModified.Remove(OnVesselModified);
            GameEvents.onVesselChange.Remove(OnVesselChange);

            RemoveMagicTransmitter(false);
        }




        #region events


        /// <summary>
        /// Player is swapping vessels
        /// </summary>
        /// <param name="v"></param>
        public void OnVesselChange(Vessel v)
        {
            Log.Debug("StorageCache.OnVesselChange to {0}", v.vesselName);

            RemoveMagicTransmitter();
            vessel = v;
            ScheduleRebuild();
        }



        public void OnVesselModified(Vessel v)
        {
            Log.Debug("StorageCache.OnVesselModified");

            if (vessel != v)
            {
                OnVesselChange(v);
            }
            else ScheduleRebuild();
        }



        public void OnVesselDestroyed(Vessel v)
        {
            if (vessel == v)
            {
                Log.Debug("StorageCache.OnVesselDestroyed");

                //RemoveMagicTransmitter();
                        // temporary: seeing if attempting to remove transmitter while
                        // vessel is being destroyed is causing:
                        //      Destroying object multiple times. Don't use DestroyImmediate on the same object in OnDisable or OnDestroy."

                storage = new StorageList();
                magicTransmitter = null;
                vessel = null;
            }
        }

        #endregion



        /// <summary>
        /// It turns out that immediately trying to cache storage containers
        /// right after a vessel switch doesn't work well; some may not be
        /// initialized.  Luckily, we can fire up a coroutine that will wait
        /// until the time is right and sort things for us.
        /// </summary>
        public void ScheduleRebuild()
        {
            if (IsBusy)
            {
                try
                {
                    StopCoroutine("Rebuild");
                }
                catch { }
            }

            StartCoroutine("Rebuild");
        }



        /// <summary>
        /// Rebuild storage cache.  This object is shared between
        /// all ExperimentObservers and allows them to determine what data
        /// is onboard the craft, being transmitted or has been submitted to
        /// R&D
        /// </summary>
        private System.Collections.IEnumerator Rebuild()
        {
            IsBusy = true;

            Log.Debug("StorageCache: Rebuilding ...");

            yield return new WaitForFixedUpdate();

            if (FlightGlobals.ActiveVessel != vessel)
            {
                // this could be an indication that we're not monitoring
                // the active vessel properly
                Log.Error("StorageCache: Active vessel is not monitored vessel.");


                RemoveMagicTransmitter();
                vessel = FlightGlobals.ActiveVessel;
            }

            while (!FlightGlobals.ready || !vessel.loaded)
            {
                Log.Debug("StorageCache.Rebuild - waiting");
                yield return new WaitForFixedUpdate();
            }


            // ScienceContainers are straightforward ...
            storage = vessel.FindPartModulesImplementing<IScienceDataContainer>();
            Log.Debug("StorageCache: located {0} IScienceDataContainers", storage.Count);


            // now we must deal with IScienceDataTransmitters, which are not 
            // quite as simple due to the MagicDataTransmitter we're faking

                // remove any existing magic transmitters
                //   note: we include non-root parts here because it's possible
                //         for two vessels to have merged, due to docking for instance.
                // 
                //          We could theoretically only remove MagicTransmitters from
                //          the root parts of docked vessels, but there may be cases
                //          where vessels that couldn't normally dock do (some kind of plugin perhaps)
                //          so I've opted for a general solution here
                RemoveMagicTransmitter(false);   

            
                // count the number of "real" transmitters onboard
                List<IScienceDataTransmitter> transmitters = FlightGlobals.ActiveVessel.FindPartModulesImplementing<IScienceDataTransmitter>();
                transmitters.RemoveAll(tx => tx is MagicDataTransmitter);

            
                if (transmitters.Count > 0)
                {
                    // as long as at least one transmitter is "real", the
                    // vessel's root part should have a MagicDataTransmitter
                    if (transmitters.Any(tx => !(tx is MagicDataTransmitter)))
                    {
                        magicTransmitter = vessel.rootPart.AddModule("MagicDataTransmitter") as MagicDataTransmitter;
                        magicTransmitter.cacheOwner = this;

                        Log.Debug("Added MagicDataTransmitter to root part {0}", FlightGlobals.ActiveVessel.rootPart.ConstructID);

                    }
                } else
                {
                    Log.Debug("Vessel {0} has no transmitters; no magic transmitter added", vessel.name);
                }

            IsBusy = false;
            Log.Debug("Rebuilt StorageCache");
        }



        /// <summary>
        /// Remove the magic transmitter (if it exists) from the given Vessel.
        /// Mainly used so that only the active vessel has a magic transmitter.  
        /// 
        /// There's a chance that in specific circumstances, having an unmonitored
        /// magic transmitter might cause problems.
        /// 
        /// </summary>
        /// <param name="v"></param>
        /// <param name="rootOnly"></param>
        private void RemoveMagicTransmitter(bool rootOnly = true)
        {
            if (vessel != null)
                if (vessel.rootPart != null)
                {
                    try
                    {
                        if (vessel.rootPart.Modules.Contains("MagicDataTransmitter"))
                            vessel.rootPart.RemoveModule(vessel.rootPart.Modules.OfType<MagicDataTransmitter>().Single());

                        if (!rootOnly)
                            foreach (var part in vessel.Parts)
                                if (part.Modules.Contains("MagicDataTransmitter"))
                                    part.RemoveModule(part.Modules.OfType<MagicDataTransmitter>().First());
                    } catch (Exception e)
                    {
                        Log.Warning("RemoveMagicTransmitter: caught exception {0}", e);
                    }
                }

            magicTransmitter = null;
        }


        /// <summary>
        /// As you might imagine, there are definitely cases where an experiment
        /// is "available" and no science is submitted, yet the data has already
        /// been collected.  For example, a vessel has two mystery goo containers.
        /// One is activated in a given situation.  That data is saved, not transmitted.
        /// Since there is another experiment available and technically no science
        /// has been collected for the experiment, should the indicator light for
        /// goo be on?  I say no.  That means we must account for any existing stored
        /// data in any container onboard the vessel.
        /// 
        /// Another note: when data is being transmitted, it is moved from a container
        /// into a transmitter module.  That's going to be a problem if we don't also
        /// include transmitters in our search for data; otherwise, as soon as the 
        /// player begins transmitting it looks like no data for the experiment is 
        /// onboard and the experiment will "become available" for a few seconds 
        /// until the data is submitted.
        ///    However, we encounter another problem here: there's no way to get at
        /// the queued ScienceData in the transmitter.  We can trick the game into
        /// always choosing our fake transmitter and get the data that way instead,
        /// though.  More detail in MagicDataTransmitter description.
        /// </summary>
        /// <param name="subjectid"></param>
        /// <param name="refData"></param>
        /// <returns></returns>
        public List<ScienceData> FindStoredData(string subjectid)
        {
            var found = new List<ScienceData>();

            foreach (var container in storage)
                foreach (var data in container.GetData())
                    if (data.subjectID == subjectid)
                        found.Add(data);
                    

            if (magicTransmitter != null)
                foreach (var data in magicTransmitter.QueuedData)
                    if (data.subjectID == subjectid)
                    {
                        found.Add(data);
                        Log.Debug("Found stored data in transmitter queue");
                    }

            return found;
        }


#if DEBUG
        public void DumpContainerData()
        {
            Log.Debug("Dumping container data...");

            if (storage == null)
                Log.Error("storage is null");

            foreach (var container in storage)
            {
                if (container == null)
                    Log.Error("container is null");

                if (container.GetData() == null)
                {
                    Log.Error("container.GetData() is null");
                }
                else
                {
                    foreach (var data in container.GetData())
                    {
                        if (data != null)
                        {
                            Log.Debug("  Data: {0}, title {1}", data.subjectID, data.title);
                        }
                        else Log.Error("data in container.getData() is null");
                    }
                }
            }

            Log.Debug("Dumping transmitter data ...");
            if (magicTransmitter == null)
                Log.Error("magicTransmitter is null!");

            foreach (var data in magicTransmitter.QueuedData)
                Log.Debug("  Data: {0}, title {1}", data.subjectID, data.title);

            Log.Debug("Finished dumping container data!");
        }
        
#endif

        public int StorageContainerCount
        {
            get
            {
                return storage.Count;
            }
        }


        public bool IsBusy
        {
            get;
            private set;
        }
    }
}
