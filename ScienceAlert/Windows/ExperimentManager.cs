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
using ScienceAlert.Toolbar;
using ReeperCommon;

namespace ScienceAlert
{
    using ExperimentObserverList = List<ExperimentObserver>;

    /// <summary>
    /// ExperimentManager has been born to reduce the responsibilities of the
    /// ScienceAlert object, which has become far too unwieldy. ExperimentManager
    /// will deal with updating experiments and reporting status changes to
    /// ScienceAlert.
    /// </summary>
    class ExperimentManager : MonoBehaviour, IDrawable
    {
        private readonly int experimentMenuID = UnityEngine.Random.Range(0, int.MaxValue);
        private const float TIMEWARP_CHECK_THRESHOLD = 10f; // when the game exceeds this threshold, experiment observers
                                                            // will check their status on every frame rather than sequentially,
                                                            // one observer per frame

        // --------------------------------------------------------------------
        //    Members of ExperimentManager
        // --------------------------------------------------------------------
        private ScienceAlert scienceAlert;
        private StorageCache vesselStorage;
        private BiomeFilter biomeFilter;

        private System.Collections.IEnumerator watcher;
        private System.Collections.IEnumerator rebuilder;

        ExperimentObserverList observers = new ExperimentObserverList();
        public ScanInterface scanInterface;

        // experiment text related
        private float maximumTextLength = float.NaN;
        private Rect experimentButtonRect = new Rect(0, 0, 0, 0);

        string lastGoodBiome = string.Empty;

/******************************************************************************
 *                    Implementation Details
 ******************************************************************************/

        void Awake()
        {
            vesselStorage = gameObject.AddComponent<StorageCache>();
            biomeFilter = gameObject.AddComponent<BiomeFilter>();
            scienceAlert = gameObject.GetComponent<ScienceAlert>();

            // event setup
            GameEvents.onVesselWasModified.Add(OnVesselWasModified);
            GameEvents.onVesselChange.Add(OnVesselChanged);
            GameEvents.onVesselDestroy.Add(OnVesselDestroyed);
            GameEvents.onCrewOnEva.Add(OnCrewGoingEva);
        }



        void OnDestroy()
        {
            GameEvents.onVesselWasModified.Remove(OnVesselWasModified);
            GameEvents.onVesselChange.Remove(OnVesselChanged);
            GameEvents.onVesselDestroy.Remove(OnVesselDestroyed);
            GameEvents.onCrewOnEva.Remove(OnCrewGoingEva);
        }



        /// <summary>
        /// Either continue building list of experiment observers or begin
        /// run status updates on them
        /// </summary>
        public void Update()
        {
            if (rebuilder != null)
            {   // still working on refreshing observer list
                if (!rebuilder.MoveNext())
                    rebuilder = null;
            }
            else
            {
                if (!vesselStorage.IsBusy && watcher != null)
                {
                    if (!PauseMenu.isOpen)
                        if (watcher != null) watcher.MoveNext();
                }
            }
        }



#region GUI functions

        /// <summary>
        /// It's necessary to figure out how wide to make the available 
        /// experiment window. I know it's ugly, but CalcSize is only
        /// available in a GUI function
        /// </summary>
        public void OnGUI()
        {
            if (float.IsNaN(maximumTextLength) && observers.Count > 0 && rebuilder == null)
            {
                // construct the experiment observer list ...
                maximumTextLength = observers.Max(observer => Settings.Skin.button.CalcSize(new GUIContent(observer.ExperimentTitle + " (123)")).x);
                experimentButtonRect.width = maximumTextLength + 10f/* a little extra for report value */;

                Log.Debug("MaximumTextLength = {0}", maximumTextLength);

                // note: we can't use CalcSize anywhere but inside OnGUI.  I know
                // it's ugly, but it's the least ugly of the available alternatives
            }
        }


        /// <summary>
        /// Whichever toolbar button (stock or blizzy) is in use will call
        /// this method. Keep in mind the other Drawables are also going to
        /// get called, so only worry about our own affairs
        /// </summary>
        /// <param name="ci"></param>
        public void OnToolbarClicked(ClickInfo ci)
        {
            // if left-click and we're not already displayed...
            if (ci.button == 0)
            {
                if (scienceAlert.Button.Drawable != null && !(scienceAlert.Button.Drawable is ExperimentManager))
                {
                    return; // somebody else is open; let them handle this click
                }
                else
                {
                    AudioUtil.Play("click1");

                    if (scienceAlert.Button.Drawable is ExperimentManager)
                    {
                        // close menu
                        scienceAlert.Button.Drawable = null;
                    }
                    else scienceAlert.Button.Drawable = this;
                }
            }
            else if (scienceAlert.Button.Drawable is ExperimentManager)
            {
                // close menu
                AudioUtil.Play("click1", 0.05f); // set min delay in case other drawables open their window
                                                 // and play a sound on this event as well
                scienceAlert.Button.Drawable = null;
            }
        }



        /// <summary>
        /// Blizzy toolbar popup menu, when the toolbar button is left-clicked.
        /// The options window is separate.
        /// </summary>
        /// <param name="position"></param>
        /// <returns></returns>
        public Vector2 Draw(Vector2 position)
        {
            //if (float.IsNaN(maximumTextLength))
            //    return Vector2.zero; // text length isn't set yet

            float maxHeight = 32f * observers.Count;
            float necessaryHeight = 32f * observers.Count(obs => obs.Available);

            if (necessaryHeight > 31.9999f)
            {
                var old = GUI.skin;

                GUI.skin = Settings.Skin;

                experimentButtonRect.x = position.x;
                experimentButtonRect.y = position.y;
                //experimentButtonRect.height = necessaryHeight;

                if (!Settings.Instance.DisplayCurrentBiome)
                {
                    experimentButtonRect = KSPUtil.ClampRectToScreen(GUILayout.Window(experimentMenuID, experimentButtonRect, DrawWindow, "Available Experiments"));
                }
                else
                {
                    necessaryHeight += Settings.Skin.box.CalcHeight(new GUIContent("Biome: Unknown"), experimentButtonRect.width);
                    experimentButtonRect.height = necessaryHeight;

                    DrawArea();

                    //experimentButtonRect = KSPUtil.ClampRectToScreen(new Rect(experimentButtonRect.x, experimentButtonRect.y, experimentButtonRect.width, necessaryHeight));
                }
                GUI.skin = old;
            }
            else
            {
                // no experiments
                scienceAlert.Button.Drawable = null;
            }
            return new Vector2(experimentButtonRect.width, necessaryHeight);
            //return new Vector2(experimentButtonRect.width, experimentButtonRect.height);
        }


        /// <summary>
        /// Window (does not display biome info)
        /// </summary>
        /// <param name="winid"></param>
        private void DrawWindow(int winid)
        {
            DrawButtons();
        }



        /// <summary>
        /// Instead of a window title, we'll put that space to use
        /// to display current biome
        /// </summary
        private void DrawArea()
        {
            GUILayout.BeginArea(experimentButtonRect, Settings.Skin.box);
            {
                if (FlightGlobals.ActiveVessel == null)
                {
                    scienceAlert.Button.Drawable = null;
                    return;
                }

                // Biome data
                {
                    if (biomeFilter.IsBusy || !scanInterface.HaveScanData(FlightGlobals.ActiveVessel.latitude, FlightGlobals.ActiveVessel.longitude, FlightGlobals.ActiveVessel.mainBody))
                    {
                        GUILayout.Box("Biome: <working>");
                    }
                    else
                    {
                        string biome;

                        if (!biomeFilter.GetBiome(FlightGlobals.ActiveVessel.latitude * Mathf.Deg2Rad, FlightGlobals.ActiveVessel.longitude * Mathf.Deg2Rad, out biome))
                        {
                            biome = lastGoodBiome;
                        }
                        else
                        {
                            lastGoodBiome = biome;
                        }
                        GUILayout.Box(string.Format(biome));
                    }
                }
                DrawButtons();
            }
            GUILayout.EndArea();
        }



        private void DrawButtons()
        {
            GUILayout.BeginVertical(GUILayout.ExpandWidth(true));
            {
                //-----------------------------------------------------
                // Experiment list
                //-----------------------------------------------------
                foreach (var observer in observers)
                    if (observer.Available)
                    {
                        var content = new GUIContent(observer.ExperimentTitle);

                        if (Settings.Instance.ShowReportValue) content.text += string.Format(" ({0:0.#})", observer.NextReportValue);

                        if (GUILayout.Button(content, Settings.Skin.button, GUILayout.ExpandHeight(false)))
                        {
                            Log.Debug("Deploying {0}", observer.ExperimentTitle);
                            AudioUtil.Play("click2");
                            observer.Deploy();
                        }
                    }
            }
            GUILayout.EndVertical();
        }



#endregion

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
                Log.Normal("Vessel was modified; refreshing observer caches...");
                foreach (var obs in observers)
                    obs.Rebuild();
                Log.Normal("Done");
            }
        }



        public void OnVesselChanged(Vessel newVessel)
        {
            Log.Debug("OnVesselChange: {0}", newVessel.name);

            

            ScheduleRebuildObserverList();
            watcher = null;
        }



        private void OnCrewGoingEva(GameEvents.FromToAction<Part, Part> relevant)
        {
            if (Settings.Instance.ReopenOnEva && scienceAlert.Button.Drawable is ExperimentManager)
            {
                Log.Debug("ExperimentManager.OnCrewGoingEva: from {0} to {1}", relevant.from.partName, relevant.to.partName);
                StartCoroutine(WaitAndReopenList(relevant.to.vessel));
            }
        }



        /// <summary>
        /// Will wait for the specified vessel to become active and for
        /// experiments to become available before reopening experiment
        /// list
        /// </summary>
        /// <param name="timeOut"></param>
        /// <returns></returns>
        private System.Collections.IEnumerator WaitAndReopenList(Vessel target)
        {
            float start = Time.realtimeSinceStartup;
            Log.Verbose("ExperimentManager: Waiting to reopen window");

            while ((FlightGlobals.ActiveVessel != target || !observers.Any(o => o.Available)) && Time.realtimeSinceStartup - start < 2f /* 2 second timeout */)
                yield return 0;

            if (!observers.Any(o => o.Available))
            {
                Log.Warning("ExperimentManager: Waited to open list, but timed out after {0:0.#} seconds", Time.realtimeSinceStartup - start);
                yield break;
            }
            else scienceAlert.Button.Drawable = this;
        }



        public void ScheduleRebuildObserverList()
        {
            observers.Clear();
            rebuilder = RebuildObserverList();
        }



        public void OnVesselDestroyed(Vessel vessel)
        {
            try
            {
                if (FlightGlobals.ActiveVessel == vessel)
                {
                    Log.Debug("Active vessel was destroyed!");
                    observers.Clear();
                    rebuilder = null;
                    watcher = null;
                }
            }
            catch (Exception)
            {
                // rarely (usually when something has gone REALLY WRONG
                // elswhere), accessing FlightGlobals.ActiveVessel will
                // spew forth a storm of NREs
                observers.Clear();
                rebuilder = watcher = null;
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

                        // Is exciting new research available?
                        if (observer.UpdateStatus(expSituation))
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
                                        AudioUtil.Play("bubbles", 2f);

                                    break;

                                case Settings.SoundNotifySetting.Always:
                                    AudioUtil.Play("bubbles", 2f);
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
                    if (TimeWarp.CurrentRate < TIMEWARP_CHECK_THRESHOLD)
                        yield return 0; // pause until next frame


                } // end observer loop

                yield return 0;
            } // end infinite while loop
        }



        /// <summary>
        /// Each experiment observer caches relevant modules to reduce cpu
        /// time.  Whenever the vessel changes, they'll need to be updated.
        /// That's what this function does.
        /// </summary>
        /// <returns></returns>
        private System.Collections.IEnumerator RebuildObserverList()
        {
            Log.Normal("Rebuilding observer list...");

            observers.Clear();
            maximumTextLength = float.NaN;


            while (ResearchAndDevelopment.Instance == null || !FlightGlobals.ready || FlightGlobals.ActiveVessel.packed || scanInterface == null)
                yield return 0;


            // critical: there's a quiet issue where sometimes user get multiple
            //           experimentIds loaded (the one I know of at the moment is
            //           through a small bug in MM), but if that happens, GetExperimentIDs()
            //           will throw an exception and the whole plugin goes down in flames.


            try
            {
                // construct the experiment observer list ...
                foreach (var expid in ResearchAndDevelopment.GetExperimentIDs())
                    if (expid != "evaReport" && expid != "surfaceSample") // special cases
                        if (ResearchAndDevelopment.GetExperiment(expid).situationMask == 0 && ResearchAndDevelopment.GetExperiment(expid).biomeMask == 0)
                        {   // we can't monitor this experiment, so no need to clutter the
                            // ui with it
                            Log.Warning("Experiment '{0}' cannot be monitored due to zero'd situation and biome flag masks.", ResearchAndDevelopment.GetExperiment(expid).experimentTitle);

                        }
                        else observers.Add(new ExperimentObserver(vesselStorage, Settings.Instance.GetExperimentSettings(expid), biomeFilter, scanInterface, expid));

                // surfaceSample is a special case: it's technically available on any
                // crewed vessel
                observers.Add(new SurfaceSampleObserver(vesselStorage, Settings.Instance.GetExperimentSettings("surfaceSample"), biomeFilter, scanInterface));

                //if (Settings.Instance.CheckSurfaceSampleNotEva)
                //    if (Settings.Instance.GetExperimentSettings("surfaceSample").Enabled)
                //        if (!FlightGlobals.ActiveVessel.isEVA)
                //        {
                //            observers.Add(new SurfaceSampleObserver(vesselStorage, Settings.Instance.GetExperimentSettings("surfaceSample"), biomeFilter, scanInterface));
                //        }
                //        else observers.Add(new ExperimentObserver(vesselStorage, Settings.Instance.GetExperimentSettings("surfaceSample"), biomeFilter, scanInterface, "surfaceSample"));

                // evaReport is a special case.  It technically exists on any crewed
                // vessel.  That vessel won't report it normally though, unless
                // the vessel is itself an eva'ing Kerbal.  Since there are conditions
                // that would result in the experiment no longer being available 
                // (kerbal dies, user goes out on eva and switches back to ship, and
                // so on) I think it's best we separate it out into its own
                // Observer type that will account for these changes and any others
                // that might not necessarily trigger a VesselModified event
                if (Settings.Instance.GetExperimentSettings("evaReport").Enabled)
                {
                    if (Settings.Instance.EvaReportOnTop)
                    {
                        observers = observers.OrderBy(obs => obs.ExperimentTitle).ToList();
                        observers.Insert(0, new EvaReportObserver(vesselStorage, Settings.Instance.GetExperimentSettings("evaReport"), biomeFilter, scanInterface));
                    }
                    else
                    {
                        observers.Add(new EvaReportObserver(vesselStorage, Settings.Instance.GetExperimentSettings("evaReport"), biomeFilter, scanInterface));
                        observers = observers.OrderBy(obs => obs.ExperimentTitle).ToList();
                    }
                } else observers = observers.OrderBy(obs => obs.ExperimentTitle).ToList();

                watcher = UpdateObservers();

                Log.Normal("Observer list rebuilt");
            }
            catch (Exception e)
            {
                Log.Error("CRITICAL: Exception RebuildObserverList(): {0}", e);

                Log.Normal("Listing current experiment definitions:");

                // It's usually something to do with duplicate crew reports
                foreach (var node in GameDatabase.Instance.GetConfigNodes("EXPERIMENT_DEFINITION"))
                {
                    // note: avoid being too spammy by removing the results sections,
                    // those aren't going to be causing problems anyway
                    ConfigNode snipped = new ConfigNode();
                    node.CopyTo(snipped);

                    snipped.RemoveNode("RESULTS");

                    Log.Normal("{0}", snipped.ToString());
                }

                Log.Normal("Finished listing experiment definitions.");

                // find any duplicates
                HashSet<string /* id */> alreadyKnown = new HashSet<string>();

                foreach (var node in GameDatabase.Instance.GetConfigNodes("EXPERIMENT_DEFINITION"))
                {
                    if (node.HasValue("id"))
                    {
                        string id = node.GetValue("id");

                        if (!alreadyKnown.Contains(id))
                        {
                            alreadyKnown.Add(id);
                        }
                        else
                        {
                            Log.Error("Duplicate science definition found for '{0}'", id);
                        }
                    }
                    else Log.Normal("no value id found");
                }
            }
        }
#endregion



#region Message handling functions

        /// <summary>
        /// This message will be sent by ScienceAlert when the user
        /// changes scan interface types
        /// </summary>
        public void Notify_ScanInterfaceChanged()
        {
            Log.Debug("ExperimentManager.Notify_ScanInterfaceChanged");

            scanInterface = gameObject.GetComponent<ScanInterface>();
            ScheduleRebuildObserverList();
        }



        /// <summary>
        /// This message sent when toolbar has changed and re-registering
        /// for events is necessary
        /// </summary>
        public void Notify_ToolbarInterfaceChanged()
        {
            Log.Debug("ExperimentManager.Notify_ToolbarInterfaceChanged");

            scienceAlert.Button.OnClick += OnToolbarClicked;
            ScheduleRebuildObserverList(); // why? to update toolbar button state
        }

#endregion
    }
}
