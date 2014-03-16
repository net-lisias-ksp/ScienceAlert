using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace ExperimentIndicator
{
    using StorageList = List<IScienceDataContainer>;


    /// <summary>
    /// The purpose of this object is to reduce redundancy between 
    /// ExperimentObservers.  ScienceData isn't sorted by ScienceExperiment
    /// but rather by id, so it makes more sense to have one object track
    /// it than have every observer do so.
    /// 
    /// StorageCache keeps track of events and will update itself as
    /// necessary.  It will also manage the magic transmitter as required.
    /// </summary>
    internal class StorageCache : MonoBehaviour
    {
        protected StorageList storage;                      // containers for science data
        protected MagicDataTransmitter magicTransmitter;    // MagicDataTransmitter keeps an eye on any queued data for the vessel
        protected Vessel vessel;                            // which vessel this storage cache is for
        protected Dictionary<Guid, List<ManeuverNode>> sessionManeuverNodes = new Dictionary<Guid, List<ManeuverNode>>();


/******************************************************************************
 *                          Begin Implementation
 *****************************************************************************/
        public void Start()
        {
            GameEvents.onVesselChange.Add(OnVesselChange);
            GameEvents.onVesselWasModified.Add(OnVesselModified);
            GameEvents.onVesselDestroy.Add(OnVesselDestroyed);
            GameEvents.onCrewOnEva.Add(CrewGoingEva);

            vessel = FlightGlobals.ActiveVessel;
            ScheduleRebuild();
        }



        public void OnDestroy()
        {
            GameEvents.onCrewOnEva.Remove(CrewGoingEva);
            GameEvents.onVesselDestroy.Remove(OnVesselDestroyed);
            GameEvents.onVesselWasModified.Remove(OnVesselModified);
            GameEvents.onVesselChange.Remove(OnVesselChange);

            RemoveMagicTransmitter(false);
        }


        public void CrewGoingEva(GameEvents.FromToAction<Part, Part> arg)
        {
            // from = part the crew got out of
            // to = the kerbal eva part itself
            Log.Debug("Crew going eva from {0} to {1}", arg.from.ConstructID, arg.to.ConstructID);

            if (arg.from.vessel == vessel)
            {
                // let's check and see if any maneuver nodes are around
                if (vessel.patchedConicSolver.maneuverNodes.Count > 0 && Settings.Instance.SaveFlightSessionManeuverNodes)
                {
                    // yep, we've got some maneuver nodes here
                    // todo: based on option whether or not to restore maneuver nodes automatically
                    StoreManeuverNodes(vessel);
                }
            }
        }

        #region events


        /// <summary>
        /// Player is swapping vessels
        /// </summary>
        /// <param name="v"></param>
        public void OnVesselChange(Vessel v)
        {
            Log.Debug("StorageCache.OnVesselChange");

            RemoveMagicTransmitter();
            vessel = v;
            ScheduleRebuild();

            // restore maneuver nodes for this vessel, if we have some stored
            if (HasStoredManeuverNodes(v.id) && FlightGlobals.ActiveVessel == v)
                RestoreManeuverNodes(v.id);
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
            Log.Debug("StorageCache.OnVesselDestroyed");

            RemoveMagicTransmitter();
            storage = new StorageList();
            magicTransmitter = null;

            if (HasStoredManeuverNodes(v.id))
                sessionManeuverNodes[v.id].Clear();
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
            if (vessel.rootPart.Modules.Contains("MagicDataTransmitter"))
                vessel.rootPart.RemoveModule(vessel.rootPart.Modules.OfType<MagicDataTransmitter>().Single());

            if (!rootOnly)
                foreach (var part in vessel.Parts)
                    if (part.Modules.Contains("MagicDataTransmitter"))
                        part.RemoveModule(part.Modules.OfType<MagicDataTransmitter>().First());

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
        public bool FindStoredData(string subjectid, out ScienceData refData)
        {
            Log.Debug("Searching for {0}", subjectid);

            foreach (var container in storage)
                foreach (var data in container.GetData())
                {
                    Log.Debug("Comparing {0} to {1}", subjectid, data.subjectID);

                    if (data.subjectID == subjectid)
                    {
                        refData = data;
                        return true;
                    }
                }


            if (magicTransmitter != null)
            {
                Log.Debug("Queued data count: {0}", magicTransmitter.QueuedData.Count);

                foreach (var data in magicTransmitter.QueuedData)
                    if (data.subjectID == subjectid)
                    {
                        refData = data;
                        Log.Warning("Found stored data in transmitter queue");
                        return true;
                    }
            }

            refData = null;
            return false;
        }


        /// <summary>
        /// Similar to above, but searches for a list of similar science
        /// data instead.  This is used when looking for experiments
        /// that might have multiple similar reports onboard (multiple
        /// equal goo samples, for example)
        /// </summary>
        /// <param name="subjectid"></param>
        /// <returns></returns>
        //public List<ScienceData> FindStoredDataList(string subjectid)
        //{
        //    List<ScienceData> dataList = new List<ScienceData>();

        //    // regular containers
        //    foreach (var container in storage)
        //        foreach (var data in container.GetData())
        //            if (data.subjectID == subjectid)
        //                dataList.Add(data);

        //    if (magicTransmitter != null)
        //        foreach (var data in magicTransmitter.QueuedData)
        //            if (data.subjectID == subjectid)
        //                dataList.Add(data);

        //    return dataList;
        //}



        /// <summary>
        /// Although a bit outside the normal scope of this object,
        /// it seems the most logical place to put session-persistent
        /// maneuver nodes.
        /// </summary>
        /// <param name="v"></param>
        public void StoreManeuverNodes(Vessel v)
        {
            Log.Debug("Storing maneuver nodes for {0}", v.vesselName);
            sessionManeuverNodes[v.id] = v.patchedConicSolver.maneuverNodes;
        }



        public bool HasStoredManeuverNodes(Guid guid)
        {
            return sessionManeuverNodes.ContainsKey(guid) && sessionManeuverNodes[guid] != null && sessionManeuverNodes[guid].Count > 0;
        }



        /// <summary>
        /// This function kicks off the restore maneuver node coroutine.
        /// </summary>
        /// <param name="v"></param>
        public void RestoreManeuverNodes(Guid guid)
        {
            if (HasStoredManeuverNodes(guid))
            {
                Log.Debug("Scheduling maneuver node restoration for {0}", guid);
                StartCoroutine(RestoreManeuverNodeRoutine(guid));
            }
        }



        /// <summary>
        /// I've experienced issues in the past when working with maneuver nodes where
        /// adding them too quickly results in error spam.
        /// </summary>
        /// <param name="v"></param>
        /// <param name="nodes"></param>
        /// <returns></returns>
        private System.Collections.IEnumerator RestoreManeuverNodeRoutine(Guid guid)
        {
            if (!HasStoredManeuverNodes(guid))
            {
                Log.Debug("No stored nodes for vessel {0}", guid);
                yield break;
            }

            while (!FlightGlobals.ready)
                yield return new WaitForFixedUpdate();

            var nodes = sessionManeuverNodes[guid];

            Func<Guid, Vessel> findVessel = target =>
            {
                foreach (var vessel in FlightGlobals.Vessels)
                    if (vessel.id == target)
                        return vessel;
                return null;
            };

            foreach (var node in nodes)
            {
                var vessel = findVessel(guid);

                if (vessel != null)
                {
                    Log.Debug("RestoreManeuverNodeRoutine: restoring node with deltaV {0}", node.DeltaV.ToString());

                    var newNode = vessel.patchedConicSolver.AddManeuverNode(node.UT);
                    newNode.nodeRotation = node.nodeRotation;
                    newNode.DeltaV = node.DeltaV;

                    vessel.patchedConicSolver.Update();
                    vessel.patchedConicSolver.UpdateFlightPlan();

                    yield return new WaitForFixedUpdate();
                }
                else
                {
                    // the vessel ... no longer exists?  hmm
                    Log.Error("Vessel destroyed while restoring maneuver nodes.");
                    yield break;
                }
            }

            Log.Debug("Finished restoring maneuver nodes to {0}", findVessel(guid).vesselName);

            // and we can now clear the cache of those nodes
            sessionManeuverNodes[guid].Clear();
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

        public bool IsBusy
        {
            get;
            private set;
        }
    }
}
