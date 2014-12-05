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

    using ExperimentStatusChanged = ScienceAlert.API.ExperimentStatusChanged;
    using ExperimentRecoveryAlert = ScienceAlert.API.ExperimentRecoveryAlert;
    using ExperimentTransmittableAlert = ScienceAlert.API.ExperimentTransmittableAlert;
    using ExperimentSubjectChanged = ScienceAlert.API.ExperimentSubjectChanged;

    /// <summary>
    /// Responsible for updating experiment monitor status
    /// </summary>
    public class MonitorManager : MonoBehaviour
    {
        //---------------------------------------------------------------------
        // Members
        //---------------------------------------------------------------------
        System.Collections.IEnumerator watcher;                     // infinite-looping method used to update experiment monitors
        MonitorList monitors = new MonitorList();                   // monitors which will be checked sequentially

        //---------------------------------------------------------------------
        // Events
        //---------------------------------------------------------------------
        public event ExperimentStatusChanged OnExperimentStatusChanged = delegate { };
        public event ExperimentRecoveryAlert OnExperimentRecoveryAlert = delegate { };
        public event ExperimentTransmittableAlert OnExperimentTransmittableAlert = delegate { };
        public event ExperimentSubjectChanged OnExperimentSubjectChanged = delegate { };
 
/******************************************************************************
 *                    Implementation Details
 ******************************************************************************/

        #region init/deinit/MonoBehaviour
        private void Start()
        {
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
        public void RescanVessel()
        {
            monitors.Clear();

            Log.Verbose("MonitorManager: Scanning vessel for experiments");

            if (FlightGlobals.ActiveVessel == null)
            {
                Log.Verbose("Scan aborted; no active vessel");
                return;
            }

            try
            {
                var modules = FlightGlobals.ActiveVessel.FindPartModulesImplementing<ModuleScienceExperiment>();

                // note: surfaceSample and evaReport are handled in a special way, so we exclude them from
                // the basic pool
                ResearchAndDevelopment.GetExperimentIDs()
                    .Where(expid => expid != "evaReport" && expid != "surfaceSample")
                    .ToList()
                    .ForEach(expid =>
                        monitors.Add(
                            new Monitors.ExperimentMonitor(expid, ProfileManager.ActiveProfile[expid], API.ScienceAlert.VesselDataCache,
                                modules.Where(mse => mse.experimentID == expid).ToList())
                            )
                    );

                // todo: eva monitor, surface sample monitor


                Log.Normal("Scanned vessel and found {0} experiment modules", modules.Count);
            }
            catch (Exception e)
            {
                Log.Error("An exception occurred while scanning the vessel for experiment modules: {0}", e);
            }

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
                    var oldStatus = monitor.Status;
                    var oldSubject = monitor.Subject;

                    if (oldStatus != monitor.UpdateStatus(situation))
                    {
                        // new status! trigger appropriate alerts
                        OnExperimentStatusChanged(monitor.Status, oldStatus, monitor);

                        // is the recovery alert new?
                        // note: the logic here is "if the recovery flag is set AND the old
                        // status did not have this flag set"
                        if ((monitor.Status & API.ExperimentStatus.Recoverable) != 0 &&
                            (oldStatus & API.ExperimentStatus.Recoverable) == 0)
                            OnExperimentRecoveryAlert(monitor.Experiment, monitor.RecoveryValue);

                        // is the transmittable alert new?
                        if ((monitor.Status & API.ExperimentStatus.Transmittable) != 0 &&
                            (oldStatus & API.ExperimentStatus.Transmittable) == 0)
                            OnExperimentTransmittableAlert(monitor.Experiment, monitor.TransmissionValue);

                    }

                    // notify subscribers if the monitor's subject has changed
                    if (oldSubject != monitor.Subject)
                        OnExperimentSubjectChanged(monitor.Status, monitor);


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

#endregion
    }
}
