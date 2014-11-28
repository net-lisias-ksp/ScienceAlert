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
using System.Collections.ObjectModel;
using System.Linq;
using UnityEngine;
using ScienceAlert.Toolbar;
using ScienceAlert.Experiments.Observers;
using ReeperCommon;

namespace ScienceAlert.Experiments
{
    using ProfileManager = ScienceAlertProfileManager;
    using ExperimentObserverList = List<ExperimentObserver>;
    
    /// <summary>
    /// ExperimentManager has been born to reduce the responsibilities of the
    /// ScienceAlert object, which has become far too unwieldy. ExperimentManager
    /// will deal with updating experiments and reporting status changes to
    /// ScienceAlert.
    /// </summary>
    public class ExperimentManager : MonoBehaviour
    {
        // --------------------------------------------------------------------
        //    Members of ExperimentManager
        // --------------------------------------------------------------------
        private ScienceAlert scienceAlert;
        private StorageCache vesselStorage;
        private BiomeFilter biomeFilter;

        private System.Collections.IEnumerator watcher;

        ExperimentObserverList observers = new ExperimentObserverList();

        string lastGoodBiome = string.Empty; // if BiomeFilter tells us the biome it got is probably not real, then we can use
                                             // this stored last known good biome instead


        // --------------------------------------------------------------------
        //    Audio
        // --------------------------------------------------------------------
        new AudioPlayer audio;



/******************************************************************************
 *                    Implementation Details
 ******************************************************************************/

        void Awake()
        {
            vesselStorage = gameObject.AddComponent<StorageCache>();
            biomeFilter = gameObject.AddComponent<BiomeFilter>();
            scienceAlert = gameObject.GetComponent<ScienceAlert>();
            audio = GetComponent<AudioPlayer>() ?? AudioPlayer.Audio;

            scienceAlert.OnScanInterfaceChanged += OnScanInterfaceChanged;
            scienceAlert.OnToolbarButtonChanged += OnToolbarButtonChanged;

            // event setup
            GameEvents.onVesselWasModified.Add(OnVesselWasModified);
            GameEvents.onVesselChange.Add(OnVesselChanged);
            GameEvents.onVesselDestroy.Add(OnVesselDestroyed);
        }



        void OnDestroy()
        {
            GameEvents.onVesselWasModified.Remove(OnVesselWasModified);
            GameEvents.onVesselChange.Remove(OnVesselChanged);
            GameEvents.onVesselDestroy.Remove(OnVesselDestroyed);
        }



        
        /// <summary>
        /// Either continue building list of experiment observers or begin
        /// run status updates on them
        /// </summary>
        public void Update()
        {
            if (FlightGlobals.ActiveVessel != null)
                if (!vesselStorage.IsBusy && watcher != null)
                {
                    if (!PauseMenu.isOpen)
                        if (watcher != null) watcher.MoveNext();
                }
        }



#region Event functions

        /// <summary>
        /// Something about the ship has changed. If it was say 
        /// an experiment being ripped off by a collision, the observer
        /// watching that experiment should probably handle that.
        /// </summary>
        /// <param name="vessel"></param>
        public void OnVesselWasModified(Vessel vessel)
        {
            if (vessel == FlightGlobals.ActiveVessel)
            {
                Log.Normal("ExperimentManager.OnVesselWasModified: rescanning vessel for experiment modules");
                foreach (var obs in observers)
                    obs.Rescan();

                Log.Normal("Done");
            }
        }



        public void OnVesselChanged(Vessel newVessel)
        {
            Log.Debug("ExperimentManager.OnVesselChange: {0}", newVessel.name);
            RebuildObserverList();
        }




        public void OnVesselDestroyed(Vessel vessel)
        {

            try
            {
                // note: on shutdown, accessing FlightGlobals.ActiveVessel will result in a NRE
                if (FlightGlobals.fetch != null && FlightGlobals.ActiveVessel == vessel)
                {
                    Log.Debug("Active vessel was destroyed!");
                    observers.Clear();
                    watcher = null;
                }
            }
            catch (Exception e)
            {
                Log.Error("Something has gone really wrong in ExperimentManager.OnVesselDestroyed: {0}", e);

                // rarely (usually when something has gone REALLY WRONG
                // elswhere), accessing FlightGlobals.ActiveVessel will
                // spew forth a storm of NREs
                observers.Clear();
                watcher = null;
            }
        }

#endregion

#region Experiment functions

        /// <summary>
        /// Update state of all experiment observers.  If their status has 
        /// changed, UpdateStatus will return true.
        /// </summary>
        /// <returns></returns>
        private System.Collections.IEnumerator UpdateObservers()
        {

            while (true)
            {
                if (!FlightGlobals.ready || FlightGlobals.ActiveVessel == null)
                {
                    yield return 0;
                    continue;
                }

                // if any new experiments become available, our state
                // changes (remember: observers return true only if their observed
                // experiment wasn't available before and just become available this update)
                var expSituation = ScienceUtil.GetExperimentSituation(FlightGlobals.ActiveVessel);

                foreach (var observer in observers)
                {
                    try
                    {
#if PROFILE
                    float start = Time.realtimeSinceStartup;
#endif
                        bool newReport = false;

                        // Is exciting new research available?
                        if (observer.UpdateStatus(expSituation, out newReport))
                        {
                            // if we're timewarping, resume normal time if that setting
                            // was used
                            if (observer.StopWarpOnDiscovery || Settings.Instance.GlobalWarp == Settings.WarpSetting.GlobalOn)
                                if (Settings.Instance.GlobalWarp != Settings.WarpSetting.GlobalOff)
                                    if (TimeWarp.CurrentRateIndex > 0)
                                    {
                                        // Simply setting warp index to zero causes some kind of
                                        // accuracy problem that can seriously affect the
                                        // orbit of the vessel.
                                        //
                                        // to avoid this, we'll take a snapshot of the orbit
                                        // pre-warp and then apply it again after we've changed
                                        // the warp rate
                                        OrbitSnapshot snap = new OrbitSnapshot(FlightGlobals.ActiveVessel.GetOrbitDriver().orbit);
                                        TimeWarp.SetRate(0, true);
                                        FlightGlobals.ActiveVessel.GetOrbitDriver().orbit = snap.Load();
                                        FlightGlobals.ActiveVessel.GetOrbitDriver().orbit.UpdateFromUT(Planetarium.GetUniversalTime());
                                    }




                            // the button is important; if it's auto-hidden we should
                            // show it to the player
                            scienceAlert.Button.Important = true;


                            if (observer.settings.AnimationOnDiscovery)
                            {
                                scienceAlert.Button.PlayAnimation();
                            }
                            else if (scienceAlert.Button.IsNormal) scienceAlert.Button.SetLit();

                            switch (Settings.Instance.SoundNotification)
                            {
                                case Settings.SoundNotifySetting.ByExperiment:
                                    if (observer.settings.SoundOnDiscovery)
                                        audio.PlayUI("bubbles", 2f);

                                    break;

                                case Settings.SoundNotifySetting.Always:
                                    audio.PlayUI("bubbles", 2f);
                                    break;
                            }
                        }
                        else if (!observers.Any(ob => ob.Available))
                        {
                            // if no experiments are available, we should be looking
                            // at a starless flask in the menu.  Note that this is
                            // in an else statement because if UpdateStatus just
                            // returned true, we know there's at least one experiment
                            // available this frame
                            //Log.Debug("No observers available: resetting state");

                            scienceAlert.Button.SetUnlit();
                            scienceAlert.Button.Important = false;
                        }
#if PROFILE
                    Log.Warning("Tick time ({1}): {0} ms", (Time.realtimeSinceStartup - start) * 1000f, observer.ExperimentTitle);
#endif
                    } catch (Exception e)
                    {
                        Log.Debug("ExperimentManager.UpdateObservers: exception {0}", e);
                    }

                    // if the user accelerated time it's possible to have some
                    // experiments checked too late. If the user is time warping
                    // quickly enough, then we'll go ahead and check every 
                    // experiment on every loop
                    if (TimeWarp.CurrentRate < Settings.Instance.TimeWarpCheckThreshold)
                        yield return 0; // pause until next frame


                } // end observer loop

                yield return 0;
            } // end infinite while loop
        }



        /// <summary>
        /// Recreates all ExperimentObservers. This is done so that we never have any ExperimentObservers
        /// that watch for experiments that the current Vessel doesn't have, except in special cases like
        /// EVA reports or surface samples.
        /// </summary>
        /// <returns>Number of observers created</returns>
        public int RebuildObserverList()
        {
            Log.Normal("Rebuilding observer list");

            observers.Clear();
            ScanInterface scanInterface = GetComponent<ScanInterface>();

            if (scanInterface == null)
                Log.Error("ExperimentManager.RebuildObserverList: No ScanInterface component found"); // this is bad; things won't break if the scan interface
                                                                                                      // is the default but there should always be a ScanInterface-type
                                                                                                      // script attached to this GO

            // construct the experiment observer list ...
            foreach (var expid in ResearchAndDevelopment.GetExperimentIDs())
                if (expid != "evaReport" && expid != "surfaceSample") // special cases
                    if (ResearchAndDevelopment.GetExperiment(expid).situationMask == 0 && ResearchAndDevelopment.GetExperiment(expid).biomeMask == 0)
                    {   // we can't monitor this experiment, so no need to clutter the
                        // ui with it
                        Log.Verbose("Experiment '{0}' cannot be monitored due to zero'd situation and biome flag masks.", ResearchAndDevelopment.GetExperiment(expid).experimentTitle);

                    }
                    else if (FlightGlobals.ActiveVessel.FindPartModulesImplementing<ModuleScienceExperiment>().Any(mse => mse.experimentID == expid))
                    {
                        // only add this observer if at least one applicable experiment is onboard
                        observers.Add(new ExperimentObserver(vesselStorage, ProfileManager.ActiveProfile[expid], biomeFilter, scanInterface, expid));
                    }

            // surfaceSample is a special case: it's technically available on any
            // crewed vessel
            observers.Add(new SurfaceSampleObserver(vesselStorage, ProfileManager.ActiveProfile["surfaceSample"], biomeFilter, scanInterface));


            // evaReport is a special case.  It technically exists on any crewed
            // vessel.  That vessel won't report it normally though, unless
            // the vessel is itself an eva'ing Kerbal.  Since there are conditions
            // that would result in the experiment no longer being available 
            // (kerbal dies, user goes out on eva and switches back to ship, and
            // so on) I think it's best we separate it out into its own
            // Observer type that will account for these changes and any others
            // that might not necessarily trigger a VesselModified event
            try
            {
                if (ProfileManager.ActiveProfile["evaReport"].Enabled)
                {
                    if (Settings.Instance.EvaReportOnTop)
                    {
                        observers = observers.OrderBy(obs => obs.ExperimentTitle).ToList();
                        observers.Insert(0, new EvaReportObserver(vesselStorage, ProfileManager.ActiveProfile["evaReport"], biomeFilter, scanInterface));
                    }
                    else
                    {
                        observers.Add(new EvaReportObserver(vesselStorage, ProfileManager.ActiveProfile["evaReport"], biomeFilter, scanInterface));
                        observers = observers.OrderBy(obs => obs.ExperimentTitle).ToList();
                    }
                }
                else observers = observers.OrderBy(obs => obs.ExperimentTitle).ToList();
            }
            catch (NullReferenceException e)
            {
                // this is another one of those things that should never happen but if they did
                // it'd be in a quiet "why isn't this list sorted?" way
                Log.Error("ExperimentManager.RebuildObserverList: Active profile does not seem to have an \"evaReport\" entry; {0}", e);
            }

            watcher = UpdateObservers(); // to prevent any problems by rebuilding in the middle of enumeration

            return observers.Count;
        }

#endregion



#region Message handling functions

        /// <summary>
        /// This message will be sent by ScienceAlert when the user
        /// changes scan interface types
        /// </summary>
        private void OnScanInterfaceChanged()
        {
            Log.Debug("ExperimentManager.OnScanInterfaceChanged");
            RebuildObserverList();
        }


        /// <summary>
        /// Event sent from ScienceAlert whenever the too--well you get it
        /// </summary>
        private void OnToolbarButtonChanged()
        {
            Log.Debug("ExperimentManager.OnToolbarButtonChanged");
            RebuildObserverList();
        }

#endregion


        #region properties

        public ReadOnlyCollection<ExperimentObserver> Observers
        {
            get
            {
                return new ReadOnlyCollection<ExperimentObserver>(observers);
            }
        }

        public BiomeFilter BiomeFilter
        {
            get
            {
                return biomeFilter;
            }
        }

        public StorageCache Storage
        {
            get
            {
                return vesselStorage;
            }
        }


        #endregion
    }
}
