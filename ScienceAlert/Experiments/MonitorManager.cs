using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using ReeperCommon;

namespace ScienceAlert.Experiments
{
    using Data;
    using ProfileManager = ScienceAlertProfileManager;
    using MonitorList = List<Experiments.Monitors.ExperimentMonitor>;


    /// <summary>
    /// Responsible for updating experiment monitor status
    /// </summary>
    public class MonitorManager : MonoBehaviour
    {
        //---------------------------------------------------------------------
        // Members
        //---------------------------------------------------------------------
        BiomeFilter filter;                                         // this is used to judge how accurate biomes are. In some cases, the way colors are interpolated
                                                                    // on the CelestialBody's attribute map causes incorrect results. Users only see this in very rare cases
                                                                    // but for us it's a problem

        Data.ScienceDataCache storage;                              // can be queried for stored ScienceData

        System.Collections.IEnumerator watcher;                     // infinite-looping method used to update experiment monitors
        MonitorList monitors = new MonitorList();                   // monitors which will be checked sequentially
        string lastKnownGoodBiome = "";                             // If the biome filter tells us we've got a dodgy biome, we can
                                                                    // fall back to this previously-good one

/******************************************************************************
 *                    Implementation Details
 ******************************************************************************/

        #region init/deinit/MonoBehaviour
        private void Start()
        {
            filter = gameObject.AddComponent<BiomeFilter>();
            storage = gameObject.AddComponent<ScienceDataCache>();

            // event setup
            GameEvents.onVesselWasModified.Add(OnVesselEvent);
            GameEvents.onVesselChange.Add(OnVesselEvent);
            GameEvents.onVesselDestroy.Add(OnVesselEvent);

            // create monitor list
            RescanVessel();
        }



        private void OnDestroy()
        {
            GameEvents.onVesselWasModified.Remove(OnVesselEvent);
            GameEvents.onVesselChange.Remove(OnVesselEvent);
            GameEvents.onVesselDestroy.Remove(OnVesselEvent);

            if (filter != null) Component.Destroy(filter);
            if (storage != null) Component.Destroy(storage);
        }


        private void Update()
        {
            if (FlightGlobals.ActiveVessel != null && watcher != null)
                watcher.MoveNext(); // watcher never ends so no need to check return value here
        }

        #endregion



        /// <summary>
        /// Refreshes monitor list and their assigned ModuleScienceExperiments
        /// </summary>
        private void RescanVessel()
        {
            monitors.Clear();

            Log.Verbose("MonitorManager: Scanning vessel for experiments");

            if (FlightGlobals.ActiveVessel == null)
            {
                Log.Verbose("Scan aborted; no active vessel");
                return;
            }

            var modules = FlightGlobals.ActiveVessel.FindPartModulesImplementing<ModuleScienceExperiment>();
            
            // note: surfaceSample and evaReport are handled in a special way, so we exclude them from
            // the basic pool
            ResearchAndDevelopment.GetExperimentIDs().Where(expid => expid != "evaReport" && expid != "surfaceSample").ToList().ForEach(expid =>
                    monitors.Add(
                        new Monitors.ExperimentMonitor( this, 
                                                        expid, 
                                                        ProfileManager.ActiveProfile[expid], 
                                                        storage, 
                                                        modules.Where(mse => mse.experimentID == expid).ToList())
                                                       )
            );

            // todo: eva monitor, surface sample monitor


            // start update loop over
            watcher = UpdateMonitors();
        }



        private System.Collections.IEnumerator UpdateMonitors()
        {
            ExperimentSituations situation;

            while (true)
            {
                situation = ScienceUtil.GetExperimentSituation(FlightGlobals.ActiveVessel);

                foreach (var monitor in monitors)
                {
                    // todo: check monitor status

                    // if the user accelerated time it's possible to have some
                    // experiments checked too late. If the user is time warping
                    // quickly enough, then we'll go ahead and check every 
                    // experiment on every loop
                    if (TimeWarp.CurrentRate < Settings.Instance.TimeWarpCheckThreshold)
                    {
                        yield return 0; // pause until next frame
                        situation = ScienceUtil.GetExperimentSituation(FlightGlobals.ActiveVessel); // for correctness' sake, otherwise we could lag up to (1-expcount) frames behind
                    }
                } // end foreach monitors


                yield return 0;
            } // end infinite while loop
        }


       
        #region GameEvents


        private void OnVesselEvent(Vessel v)
        {
#if DEBUG
            Log.Debug("OnVesselEvent for {0}", v.vesselName);
#endif

            if (FlightGlobals.ActiveVessel == v && v != null)
            {
                RescanVessel();
            }
#if DEBUG
            else
            {
                // just to be sure
                Log.Debug("MonitorManager.OnVesselEvent: ignoring event for vessel {0}", v.vesselName);
            }
#endif
        }



        #endregion


        #region properties
        public BiomeFilter BiomeFilter
        {
            get
            {
                return filter;
            }
        }

        public Experiments.Data.ScienceDataCache Storage
        {
            get
            {
                return storage;
            }
        }
#endregion
    }
}
