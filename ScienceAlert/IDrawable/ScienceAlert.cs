/******************************************************************************
 *                  Science Alert for Kerbal Space Program                    *
 *                                                                            *
 * Version 1.5                                                               *
 * Author: xEvilReeperx                                                       *
 * Created: 3/3/2014                                                          *
 * ************************************************************************** *
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
 ****************************************************************************
 * Changelog
 * 1.5
 *      Fixed an issue where game would use the incorrect camera
 *          after ejecting an eva kerbal, potentially causing a crash
 *          
 * 1.4
 *      Fixed an issue that could cause ScienceAlert to spew NREs when
 *          the orbited body didn't have a biome map
 *          
 * 1.3
 *      Fixed a serious issue: ScienceAlert wasn't taking science
 *          multipliers into account correctly which could result in
 *          incorrect alerts
 *          
 *      Added a global warp setting option
 *      
 *      Added a global alert sound setting option
 *      
 *      Log spam due to vessel modification reduced
 * 
 *      Experiments that rely on custom code to determine availability
 *          will now be hidden from the options window, since ScienceAlert
 *          will be unable to interact with them (determined by checking
 *          if that experiment's biome and situation mask flags are zero)
 * 
 * 
 * 1.2
 *      Fixed a serious issue: "stop on discovery" could sometimes affect
 *          orbital parameters, especially at high warp
 *          
 *      Further tweaked biome detection. There should no longer be any
 *          instances of incorrect science alerts
 * 
 * 1.1
 *      Added EVA condition warning
 *      
 *      Fixed issue where base function of ModuleScienceExperiment was called
 *          instead of the correct one
 *          
 *      Fixed issue where ScienceAlert would detect the wrong biome and flash
 *          the icon at the incorrect time
 *          
 *      Stock experiments no longer have "Assume onboard" option display
 *          by default (to reduce clutter)
 *          
 *      Temporarily added a debug menu, accessable using default settings by
 *          middle-mouse clicking the toolbar button
 *          
 *      Available experiment menu polished a little; will now appear in the
 *          same location consistently
 * 
 * 1.0
 *      First public release
 *****************************************************************************/

//#define PROFILE
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Toolbar;
using DebugTools;
using ResourceTools;


// TODO: manually selected transmitters ignore MagicTransmitter
//          further thought: who really does this?  will think on it more

// TODO: properly figure out total science value of all onboard reports
//          (even multiple copies)

// todo: separate science observer for surface samples like the eva one?

// BUG: if an eva report is stored and the player goes on eva in the same
// situation, ScienceAlert reports that an eva report is available
//      haven't been able to verify this one




namespace ScienceAlert
{
    using ExperimentObserverList = List<ExperimentObserver>;


    /// <summary>
    /// Combination of animation and sound effects on status
    /// changes for experiment observers
    /// </summary>
    class EffectController
    {
        private const string NormalFlaskTexture = "ScienceAlert/textures/flask";
        private List<string> StarFlaskTextures = new List<string>();

        private float FrameRate = 24f;
        private int FrameCount = 100;
        private int CurrentFrame = 0;
        private float ElapsedTime = 0f;
        private ScienceAlert indicator;

        AudioController audio = new AudioController();

        public EffectController(ScienceAlert indi)
        {
            Func<int, int, string> GetFrame = delegate(int frame, int desiredLen)
            {
                string str = frame.ToString();

                while (str.Length < desiredLen)
                    str = "0" + str;

                return str;
            };



            // load textures
            try
            {
                if (!GameDatabase.Instance.ExistsTexture(NormalFlaskTexture))
                {
                    // load normal flask texture
                    Log.Debug("Loading normal flask texture");

                    #region normal flask texture
                    Texture2D nflask = ResourceUtil.GetEmbeddedTexture("ScienceAlert.Resources.flask.png", true);

                    if (nflask == null)
                    {
                        Log.Error("Failed to create normal flask texture!");
                    }
                    else
                    {
                        GameDatabase.TextureInfo ti = new GameDatabase.TextureInfo(nflask, false, true, true);
                        ti.name = NormalFlaskTexture;
                        GameDatabase.Instance.databaseTexture.Add(ti);
                        Log.Debug("Created normal flask texture {0}", ti.name);
                    }

                    #endregion
                    #region sprite sheet textures


                    // load sprite sheet
                    Log.Debug("Loading sprite sheet textures...");

                    Texture2D sheet = ResourceUtil.GetEmbeddedTexture("ScienceAlert.Resources.sheet.png", false, false);

                    if (sheet == null)
                    {
                        Log.Error("Failed to create sprite sheet texture!");
                    }
                    else
                    {
                        var rt = RenderTexture.GetTemporary(sheet.width, sheet.height);
                        var oldRt = RenderTexture.active;
                        int invertHeight = ((FrameCount - 1) / (sheet.width / 24)) * 24;

                        Graphics.Blit(sheet, rt);
                        RenderTexture.active = rt;

                        for (int i = 0; i < FrameCount; ++i)
                        {
                            StarFlaskTextures.Add(NormalFlaskTexture + GetFrame(i + 1, 4));
                            Texture2D sliced = new Texture2D(24, 24, TextureFormat.ARGB32, false);

                            //Log.Debug("{0} = {1},{2}", i, (i % (sheet.width / 24)) * 24, invertHeight - (i / (sheet.width / 24)) * 24);

                            sliced.ReadPixels(new Rect((i % (sheet.width / 24)) * 24, /*invertHeight -*/ (i / (sheet.width / 24)) * 24, 24, 24), 0, 0);
                            sliced.Apply();

                            //sliced.SaveToDisk(StarFlaskTextures.Last());

                            GameDatabase.TextureInfo ti = new GameDatabase.TextureInfo(sliced, false, false, false);
                            ti.name = StarFlaskTextures.Last();

                            GameDatabase.Instance.databaseTexture.Add(ti);
                            Log.Debug("Added sheet texture {0}", ti.name);
                        }

                        //sheet.SaveToDisk("ScienceAlert/textures/sheet.png");

                        RenderTexture.active = oldRt;
                        RenderTexture.ReleaseTemporary(rt);
                    }

                    Log.Debug("Finished loading sprite sheet textures.");
                    #endregion
                }
                else
                { // textures already loaded
                    for (int i = 0; i < FrameCount; ++i)
                        StarFlaskTextures.Add(NormalFlaskTexture + GetFrame(i + 1, 4));
                }
            } catch (Exception e)
            {
                Log.Error("Failed to load textures: {0}", e);
            }
            


            indicator = indi;
            indicator.ToolbarButton.TexturePath = NormalFlaskTexture;

            CurrentFrame = 0;
            ElapsedTime = 0f;
            FrameRate = Settings.Instance.StarFlaskFrameRate;
        }



        public System.Collections.IEnumerator FlaskAnimation()
        {
            while (true)
            {
                //.Debug("FlaskAnimation, current state {0}", indicator.State);

                if (indicator)
                    switch (indicator.State)
                    {
                        case ScienceAlert.IconState.NewResearch:
                            if (StarFlaskTextures.Count > 0)
                            {
                                if (ElapsedTime > TimePerFrame)
                                {
                                    while (ElapsedTime > TimePerFrame)
                                    {
                                        ElapsedTime -= TimePerFrame;
                                        CurrentFrame = (CurrentFrame + 1) % StarFlaskTextures.Count;
                                    }

                                    indicator.ToolbarButton.TexturePath = StarFlaskTextures[CurrentFrame];
                                }
                                else
                                {
                                    ElapsedTime += Time.deltaTime;
                                }
                            }
                            else
                            {
#if DEBUG
                                Log.Error("No star flask animation frames loaded.");
#endif
                                indicator.ToolbarButton.TexturePath = NormalFlaskTexture; // this is an error; we should have frames loaded
                            }
                            break;

                        case ScienceAlert.IconState.ResearchAvailable:
                            indicator.ToolbarButton.TexturePath = StarFlaskTextures[0];
                            break;

                        case ScienceAlert.IconState.NoResearch:
                            indicator.ToolbarButton.TexturePath = NormalFlaskTexture;
                            break;
                    }

                yield return null;
            }
        }



        /// <summary>
        /// Determine whether to play a sound and/or change the indicator's
        /// IconStatus based on the newly-available status of experiment
        /// </summary>
        /// <param name="experiment"></param>
        public void OnExperimentAvailable(ExperimentObserver experiment)
        {
            //Log.Verbose("OnExperimentAvailable: Experiment {0} just become available!", experiment.ExperimentTitle);

            // firstly, handle sound
            if (experiment.SoundOnDiscovery || Settings.Instance.SoundNotification == Settings.SoundNotifySetting.Always)
                if (Settings.Instance.SoundNotification != Settings.SoundNotifySetting.Never)
                    audio.PlaySound("bubbles");

            // now handle state
            if (experiment.AnimateOnDiscovery && Settings.Instance.FlaskAnimationEnabled)
            {
                indicator.State = ScienceAlert.IconState.NewResearch;
            }
            else
            {
                if (Settings.Instance.FlaskAnimationEnabled)
                {
                    // if the icon is already animated, don't stop it because this
                    // particular experiment wouldn't produce one
                    indicator.State = indicator.State == ScienceAlert.IconState.NewResearch ? ScienceAlert.IconState.NewResearch : ScienceAlert.IconState.ResearchAvailable;
                }
                else indicator.State = ScienceAlert.IconState.ResearchAvailable;
            }

            // menu status (open/closed) is handled by ScienceAlert;
            // we needn't bother with its effects on state here
        }



        private float TimePerFrame
        {
            get
            {
                return 1f / FrameRate;
            }
        }

        public AudioController AudioController
        {
            get
            { return audio; }
        }
    }



    [KSPAddon(KSPAddon.Startup.Flight, false)]
    public class ScienceAlert : MonoBehaviour, IDrawable
    {
        public enum IconState
        {
            NoResearch,             // no experiments available, basic flask icon

            NewResearch,            // spinning star flask icon to let user know something new happened

            ResearchAvailable       // Once the user opens the menu and has a look at whatever new thing we have to show,
                                    // NewResearch becomes ResearchAvailable and the spinning star stops (but flask keeps
                                    // its star-behind icon)
        }


        private const float TIMEWARP_CHECK_THRESHOLD = 10f; // when the game exceeds this threshold, experiment observers
                                                            // will check their status on every frame rather than sequentially,
                                                            // one observer per frame

        // --------------------------------------------------------------------
        //    Members of ScienceAlert
        // --------------------------------------------------------------------
        ExperimentObserverList observers = new ExperimentObserverList();
        private StorageCache vesselStorage;
        private System.Collections.IEnumerator watcher;
        new private System.Collections.IEnumerator animation;
        private System.Collections.IEnumerator rebuilder;
        private IconState researchState = IconState.NoResearch;

        // related controls
        private Toolbar.IButton mainButton;
        private OptionsWindow optionsWindow;

        // animation and audio effects from experiment observers
        EffectController effects;

        // cleans up biome maps
        BiomeFilter biomeFilter;

        // experiment text related
        private float maximumTextLength = float.NaN;
        private Rect experimentButtonRect = new Rect(0, 0, 0, 0);
        private int experimentMenuID = UnityEngine.Random.Range(int.MinValue, int.MaxValue);

// ----------------------------------------------------------------------------
//             --> Implementation of ScienceAlert <--
// ----------------------------------------------------------------------------

        public void Start()
        {
           
            // event setup
            GameEvents.onVesselWasModified.Add(OnVesselWasModified);
            GameEvents.onVesselChange.Add(OnVesselChanged);
            GameEvents.onVesselDestroy.Add(OnVesselDestroyed);

            // toolbar setup
            mainButton = Toolbar.ToolbarManager.Instance.add("ScienceAlert", "PopupOpen");
            mainButton.Text = "Science Alert";
            mainButton.ToolTip = "Left Click to Open Experiments; Right Click for Settings";
            mainButton.OnClick += OnToolbarClick;

            effects = new EffectController(this); // requires toolbar button to exist
            optionsWindow = new OptionsWindow(effects.AudioController);
            biomeFilter = gameObject.AddComponent<BiomeFilter>();

            // set up coroutines
            rebuilder = RebuildObserverList();
            animation = effects.FlaskAnimation();
        }



        public void OnDestroy()
        {
            Log.Debug("ScienceAlert destroyed");
            mainButton.Destroy();

        }



        public void OnGUI()
        {
            if (float.IsNaN(maximumTextLength) && observers.Count > 0 && rebuilder == null)
            {
                // construct the experiment observer list ...
                maximumTextLength = observers.Max(observer => Settings.Skin.button.CalcSize(new GUIContent(observer.ExperimentTitle)).x);
                experimentButtonRect.width = maximumTextLength + 5;

                Log.Debug("MaximumTextLength = {0}", maximumTextLength);

                // note: we can't use CalcSize anywhere but inside OnGUI.  I know
                // it's ugly, but it's the least ugly of the available alternatives
            }
        }



        public void Update()
        {
            if (rebuilder != null)
            {   // still working on refreshing observer list
                if (!rebuilder.MoveNext())
                    rebuilder = null;
            }
            else
            {
                if (!vesselStorage.IsBusy && watcher != null && animation != null)
                {
                    if (!PauseMenu.isOpen)
                        watcher.MoveNext();
                    animation.MoveNext();
                } else Log.Debug("storage is busy");
            }

            //if (Input.GetKeyDown(KeyCode.S))
            //{
            //    Log.Debug("Playing bubbles!");

            //    audio.PlaySound(AudioController.AvailableSounds.Bubbles);
            //}

            //if (Input.GetKeyDown(KeyCode.U))
            //    vesselStorage.DumpContainerData();

            //if (Input.GetKeyDown(KeyCode.R))
            //    vesselStorage.ScheduleRebuild();

            //if (Input.GetKeyDown(KeyCode.U))
            //{
            //    foreach (var expid in ResearchAndDevelopment.GetExperimentIDs())
            //    {
            //        var settings = Settings.Instance.GetExperimentSettings(expid);
            //        Log.Debug("{0} settings: {1}", expid, settings.ToString());
            //    }
            //}
        }

        
        
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
                //Log.Debug("OnVesselWasModified invoked, rebuilding observer list");
                //observers.Clear();
                //rebuilder = RebuildObserverList();

                Log.Write("Vessel was modified; refreshing observer caches...");
                    foreach (var obs in observers)
                        obs.Rebuild();
                Log.Write("Done");
            }
        }



        public void OnVesselChanged(Vessel newVessel)
        {
            Log.Debug("OnVesselChange: {0}", newVessel.name);
            observers.Clear();
            rebuilder = RebuildObserverList();
            watcher = null;
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
            } catch (Exception)
            {
                // rarely (usually when something has gone REALLY WRONG
                // elswhere), accessing FlightGlobals.ActiveVessel will
                // spew forth a storm of NREs
                observers.Clear();
                rebuilder = watcher = null;
            }
        }



        /// <summary>
        /// Each experiment observer caches relevant modules to reduce cpu
        /// time.  Whenever the vessel changes, they'll need to be updated.
        /// That's what this function does.
        /// </summary>
        /// <returns></returns>
        private System.Collections.IEnumerator RebuildObserverList()
        {
            Log.Write("Rebuilding observer list...");

            observers.Clear();
            maximumTextLength = float.NaN;

            if (vesselStorage == null)
                vesselStorage = gameObject.AddComponent<StorageCache>();

            while (ResearchAndDevelopment.Instance == null || !FlightGlobals.ready || FlightGlobals.ActiveVessel.packed)
                yield return 0;

            // construct the experiment observer list ...
            foreach (var expid in ResearchAndDevelopment.GetExperimentIDs())
                if (expid != "evaReport") // evaReport is a special case
                    if (ResearchAndDevelopment.GetExperiment(expid).situationMask == 0 && ResearchAndDevelopment.GetExperiment(expid).biomeMask == 0)
                    {   // we can't monitor this experiment, so no need to clutter the
                        // ui with it
                        Log.Warning("Experiment '{0}' cannot be monitored due to zero'd situation and biome flag masks.", ResearchAndDevelopment.GetExperiment(expid).experimentTitle);
                        
                    } else observers.Add(new ExperimentObserver(vesselStorage, Settings.Instance.GetExperimentSettings(expid), biomeFilter, expid));

            // evaReport is a special case.  It technically exists on any crewed
            // vessel.  That vessel won't report it normally though, unless
            // the vessel is itself an eva'ing Kerbal.  Since there are conditions
            // that would result in the experiment no longer being available 
            // (kerbal dies, user goes out on eva and switches back to ship, and
            // so on) I think it's best we separate it out into its own
            // Observer type that will account for these changes and any others
            // that might not necessarily trigger a VesselModified event
            if (Settings.Instance.GetExperimentSettings("evaReport").Enabled)
                observers.Add(new EvaReportObserver(vesselStorage, Settings.Instance.GetExperimentSettings("evaReport"), biomeFilter));

            observers = observers.OrderBy(obs => obs.ExperimentTitle).ToList();

            watcher = UpdateObservers();

            Log.Write("Observer list rebuilt");
        }



        //private System.Collections.IEnumerator StopWarp()
        //{
        //    // if we're timewarping, resume normal time if that setting
        //    // was used
        //    if (TimeWarp.CurrentRateIndex > 0)
        //    {
        //        FlightDriver.SetPause(true);
        //        yield return new WaitForFixedUpdate();

        //        TimeWarp.SetRate(0, true);
        //        yield return new WaitForFixedUpdate();

        //        FlightDriver.SetPause(false);
        //    }

        //    yield break;
        //}

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
                //var expSituation = VesselSituationToExperimentSituation();
                var expSituation = ScienceUtil.GetExperimentSituation(FlightGlobals.ActiveVessel);

                foreach (var observer in observers)
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


                        effects.OnExperimentAvailable(observer);

                        // the button is important; if it's auto-hidden we should
                        // show it to the player
                        if (!OptionsOpen && !MenuOpen && Settings.Instance.FlaskAnimationEnabled) ToolbarButton.Important = true;

                        UpdateMenuState();
                    } else if (!observers.Any(ob => ob.Available)) {
                        // if no experiments are available, we should be looking
                        // at a starless flask in the menu.  Note that this is
                        // in an else statement because if UpdateStatus just
                        // returned true, we know there's at least one experiment
                        // available this frame
                        //Log.Debug("No observers available: resetting state");

                        State = IconState.NoResearch;

                        if (MenuOpen) // nothing to be seen
                            MenuOpen = false;
                    }
#if PROFILE
                    Log.Warning("Tick time ({1}): {0} ms", (Time.realtimeSinceStartup - start) * 1000f, observer.ExperimentTitle);
#endif
                    
                    // if the user accelerated time, it's possible to have some
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
        /// Blizzy toolbar popup menu, when the toolbar button is left-clicked.
        /// The options window is separate.
        /// </summary>
        /// <param name="position"></param>
        /// <returns></returns>
        public Vector2 Draw(Vector2 position)
        {
            if (float.IsNaN(maximumTextLength))
                return Vector2.zero; // text length isn't set yet

            float maxHeight = 32f * observers.Count;
            float necessaryHeight = 32f * observers.Count(obs => obs.Available);

            if (necessaryHeight < 31.9999f)
            {
                MenuOpen = false;
                return Vector2.zero;
            }

            var old = GUI.skin;

            GUI.skin = Settings.Skin;

            experimentButtonRect.x = position.x;
            experimentButtonRect.y = position.y;
            experimentButtonRect.height = necessaryHeight;

            GUILayout.Window(experimentMenuID, experimentButtonRect, DrawButtonMenu, "Available Experiments");


            GUI.skin = old;

            return new Vector2(experimentButtonRect.width, maxHeight);
        }


        private void DrawButtonMenu(int winid)
        {
            //GUILayout.BeginArea(new Rect(position.x, position.y, necessaryWidth, necessaryHeight));
            GUILayout.BeginVertical(GUILayout.ExpandWidth(true));

            foreach (var observer in observers)
                if (observer.Available)
                {
                    var content = new GUIContent(observer.ExperimentTitle);

                    if (GUILayout.Button(content))
                    {
                        Log.Debug("Deploying {0}", observer.ExperimentTitle);
                        effects.AudioController.PlaySound("click2");
                        observer.Deploy();
                    }
                }
            GUILayout.EndVertical();
            //GUILayout.EndArea();
        }

        public void OnToolbarClick(ClickEvent ce)
        {
            effects.AudioController.PlaySound("click1");

            if (ce.MouseButton == 0)
            {
                Log.Debug("Toolbar left clicked");

                if (!OptionsOpen)
                {
                    MenuOpen = !MenuOpen;
                }
                else OptionsOpen = false; // if options is open, left click will close it.  Another click
                                            // should be required to open the menu
            }
            else // right or middle click
            {
                if (ce.MouseButton == 2 && Settings.Instance.DebugMode) // middle
                {
                    Log.Debug("DebugMode enabled, middle-click on toolbar button. Opening debug menu.");
                    DebugOpen = !DebugOpen;
                }
                else
                {
                    Log.Debug("Toolbar right or middle clicked");

                    OptionsOpen = !OptionsOpen;
                }
            }
        }


        #region invoked functions

        private void UpdateMenuState()
        {
            // quick recap on button image logic:
            //      no experiments available at all = normal icon
            //      new experiments are available and user hasn't looked at the menu => animated star flask
            //      experiments are available and user looked at them (or is looking at them) => non-animated star flask
            if (MenuOpen)
            {
                // stop the star flask animation, if it's running
                if (State == IconState.NewResearch)
                {
                    State = IconState.ResearchAvailable;
                    ToolbarButton.Important = false;
                }
            }
        }


        #endregion


        #region properties
        public IconState State
        {
            get
            {
                return researchState;
            }

            set
            {
                researchState = value;
            }
        }

        public IButton ToolbarButton
        {
            get
            {
                return mainButton;
            }
        }

        private bool MenuOpen
        {
            get
            {
                return mainButton.Drawable != null && mainButton.Drawable is ScienceAlert;
            }
            set
            {
                mainButton.Drawable = value ? this : null;
                UpdateMenuState();
                ToolbarButton.Important = false;
            }
        }



        private bool OptionsOpen
        {
            get
            {
                return mainButton.Drawable != null && mainButton.Drawable is OptionsWindow;
            }
            set
            {
                if (value)
                {
                    mainButton.Drawable = optionsWindow;
                    ToolbarButton.Important = false;
                }
                else if (OptionsOpen)
                {
                    mainButton.Drawable = null;
                    Settings.Instance.Save();
                }
                UpdateMenuState();
            }
        }

        private bool DebugOpen
        {
            get
            {
                return mainButton.Drawable != null && mainButton.Drawable is DebugWindow;
            }
            set
            {
                if (!value)
                {
                    if (DebugOpen)
                        mainButton.Drawable = null;
                }
                else if (!DebugOpen)
                {
                    mainButton.Drawable = new DebugWindow(this, biomeFilter, vesselStorage);
                    UpdateMenuState();
                }
            }
        }

#endregion

        #region helpers
        private string GetFrame(int frameIndex)
        {
            string str = frameIndex.ToString();

            while (str.Length < 4)
                str = "0" + str;

            return str;
        }

        //public ExperimentSituations VesselSituationToExperimentSituation()
        //{
        //    //Log.Debug("Flying low altitude <= {0}, Low space altitude <= {1}",(double)FlightGlobals.ActiveVessel.mainBody.scienceValues.flyingAltitudeThreshold,  (double)FlightGlobals.ActiveVessel.mainBody.scienceValues.spaceAltitudeThreshold);

        //    switch (FlightGlobals.ActiveVessel.situation)
        //    {
        //        case Vessel.Situations.LANDED:
        //        case Vessel.Situations.PRELAUNCH:
        //            return ExperimentSituations.SrfLanded;
        //        case Vessel.Situations.SPLASHED:
        //            return ExperimentSituations.SrfSplashed;
        //        case Vessel.Situations.FLYING:
        //            if (FlightGlobals.ActiveVessel.altitude < (double)FlightGlobals.ActiveVessel.mainBody.scienceValues.flyingAltitudeThreshold)
        //                return ExperimentSituations.FlyingLow;

        //            return ExperimentSituations.FlyingHigh;

        //        default:
        //            if (FlightGlobals.ActiveVessel.altitude < (double)FlightGlobals.ActiveVessel.mainBody.scienceValues.spaceAltitudeThreshold)

        //                return ExperimentSituations.InSpaceLow;
        //            return ExperimentSituations.InSpaceHigh;
        //    }
        //}
        #endregion
    }
}
