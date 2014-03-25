//#define PROFILE
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Toolbar;
using DebugTools;

// TODO: manually selected transmitters ignore MagicTransmitter
//          further thought: who really does this?  will think on it more

// todo: separate science observer for surface samples like the eva one?


// todo: only update experiments on vessel situation change?
//          further thought: maybe not.  It's not that expensive as is, plus
//          mod experiments might be changing data on the fly



namespace ExperimentIndicator
{
    using ExperimentObserverList = List<ExperimentObserver>;


    /// <summary>
    /// Combination of animation and sound effects on status
    /// changes for experiment observers
    /// </summary>
    class EffectController
    {
        private const string NormalFlaskTexture = "ExperimentIndicator/textures/flask";
        private List<string> StarFlaskTextures = new List<string>();

        private float FrameRate = 24f;
        private int FrameCount = 100;
        private int CurrentFrame = 0;
        private float ElapsedTime = 0f;
        private ExperimentIndicator indicator;

        AudioController audio = new AudioController();

        public EffectController(ExperimentIndicator indi)
        {
            indicator = indi;
            indicator.ToolbarButton.TexturePath = NormalFlaskTexture;

            Func<int, int, string> GetFrame = delegate(int frame, int desiredLen)
            {
                string str = frame.ToString();

                while (str.Length < desiredLen)
                    str = "0" + str;

                return str;
            };

            for (int i = 0; i < FrameCount; ++i)
                StarFlaskTextures.Add(NormalFlaskTexture + GetFrame(i + 1, 4));

            CurrentFrame = 0;
            ElapsedTime = 0f;
        }



        public System.Collections.IEnumerator FlaskAnimation()
        {
            while (true)
            {
                //.Debug("FlaskAnimation, current state {0}", indicator.State);

                if (indicator)
                    switch (indicator.State)
                    {
                        case ExperimentIndicator.IconState.NewResearch:
                            if (StarFlaskTextures.Count > 0)
                            {
                                if (ElapsedTime > TimePerFrame)
                                {
                                    ElapsedTime -= TimePerFrame;
                                    CurrentFrame = (CurrentFrame + 1) % StarFlaskTextures.Count;
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

                        case ExperimentIndicator.IconState.ResearchAvailable:
                            indicator.ToolbarButton.TexturePath = StarFlaskTextures[0];
                            break;

                        case ExperimentIndicator.IconState.NoResearch:
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
            Log.Warning("OnExperimentAvailable: Experiment {0} just become available!", experiment.ExperimentTitle);

            // firstly, handle sound
            if (experiment.SoundOnDiscovery && Settings.Instance.SoundOnNewResearch)
                audio.PlaySound(AudioController.AvailableSounds.Bubbles);

            // now handle state
            if (experiment.AnimateOnDiscovery && Settings.Instance.FlaskAnimationEnabled)
            {
                indicator.State = ExperimentIndicator.IconState.NewResearch;
            }
            else
            {
                if (Settings.Instance.FlaskAnimationEnabled)
                {
                    // if the icon is already animated, don't stop it because this
                    // particular experiment wouldn't produce one
                    indicator.State = indicator.State == ExperimentIndicator.IconState.NewResearch ? ExperimentIndicator.IconState.NewResearch : ExperimentIndicator.IconState.ResearchAvailable;
                }
                else indicator.State = ExperimentIndicator.IconState.ResearchAvailable;
            }

            // menu status (open/closed) is handled by ExperimentIndicator;
            // we needn't bother with its effects on state here
        }



        private float TimePerFrame
        {
            get
            {
                return FrameRate * 0.001f;
            }
        }

        public AudioController AudioController
        {
            get
            { return audio; }
        }
    }



    [KSPAddon(KSPAddon.Startup.Flight, false)]
    public class ExperimentIndicator : MonoBehaviour, IDrawable
    {
        public enum IconState
        {
            NoResearch,             // no experiments available, basic flask icon

            NewResearch,            // spinning star flask icon to let user know something new happened

            ResearchAvailable       // Once the user opens the menu and has a look at whatever new thing we have to show,
                                    // NewResearch becomes ResearchAvailable and the spinning star stops (but flask keeps
                                    // its star-behind icon)
        }

        // --------------------------------------------------------------------
        //    Members of ExperimentIndicator
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


        // experiment text related
        private float maximumTextLength = float.NaN;

// ----------------------------------------------------------------------------
//             --> Implementation of ExperimentIndicator <--
// ----------------------------------------------------------------------------

        public void Start()
        {
           
            GameEvents.onVesselWasModified.Add(OnVesselWasModified);
            GameEvents.onVesselChange.Add(OnVesselChanged);
            GameEvents.onVesselDestroy.Add(OnVesselDestroyed);

            mainButton = Toolbar.ToolbarManager.Instance.add("ExperimentIndicator", "PopupOpen");
            mainButton.Text = "ExpIndicator";
            mainButton.ToolTip = "Left Click to Open Experiments; Right Click for Settings";
            mainButton.OnClick += OnToolbarClick;

            effects = new EffectController(this);
            optionsWindow = new OptionsWindow(effects.AudioController);

            // set up coroutines
            rebuilder = RebuildObserverList();
            animation = effects.FlaskAnimation();
        }


        public void OnDestroy()
        {
            Log.Debug("ExperimentIndicator destroyed");
            mainButton.Destroy();

        }

        public void OnGUI()
        {
            if (float.IsNaN(maximumTextLength) && observers.Count > 0 && rebuilder == null)
            {
                // construct the experiment observer list ...
                maximumTextLength = observers.Max(observer => Settings.Skin.button.CalcSize(new GUIContent(observer.ExperimentTitle)).x);
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

            if (Input.GetKeyDown(KeyCode.U))
            {
                foreach (var expid in ResearchAndDevelopment.GetExperimentIDs())
                {
                    var settings = Settings.Instance.GetExperimentSettings(expid);
                    Log.Debug("{0} settings: {1}", expid, settings.ToString());
                }
            }
        }

        
        

        public void OnVesselWasModified(Vessel vessel)
        {
            if (vessel == FlightGlobals.ActiveVessel)
            {
                Log.Debug("OnVesselWasModified invoked, rebuilding observer list");
                observers.Clear();
                rebuilder = RebuildObserverList();
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
            if (FlightGlobals.ActiveVessel == vessel)
            {
                Log.Debug("Active vessel was destroyed!");
                observers.Clear();
                rebuilder = null;
                watcher = null;
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
            observers.Clear();
            maximumTextLength = float.NaN;

            if (vesselStorage == null)
                vesselStorage = gameObject.AddComponent<StorageCache>();

            while (ResearchAndDevelopment.Instance == null || !FlightGlobals.ready || FlightGlobals.ActiveVessel.packed)
                yield return 0;

            // construct the experiment observer list ...
            foreach (var expid in ResearchAndDevelopment.GetExperimentIDs())
                if (expid != "evaReport") // evaReport is a special case
                    observers.Add(new ExperimentObserver(vesselStorage, Settings.Instance.GetExperimentSettings(expid), expid));

            // evaReport is a special case.  It technically exists on any crewed
            // vessel.  That vessel won't report it normally though, unless
            // the vessel is itself an eva'ing Kerbal.  Since there are conditions
            // that would result in the experiment no longer being available 
            // (kerbal dies, user goes out on eva and switches back to ship, and
            // so on) I think it's best we separate it out into its own
            // Observer type that will account for these changes and any others
            // that might not necessarily trigger a VesselModified event
            if (Settings.Instance.GetExperimentSettings("evaReport").Enabled)
                observers.Add(new EvaReportObserver(vesselStorage, Settings.Instance.GetExperimentSettings("evaReport")));

            observers.OrderBy(obs => obs.ExperimentTitle);

            watcher = UpdateObservers();
        }



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
                var expSituation = VesselSituationToExperimentSituation();

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
                        if (observer.StopWarpOnDiscovery)
                            if (TimeWarp.CurrentRateIndex > 0)
                                TimeWarp.SetRate(0, true);

                        effects.OnExperimentAvailable(observer);

                        // if the menu is already open, we don't need to use the flashy animation
                        //State = MenuOpen ? IconState.ResearchAvailable : IconState.NewResearch;
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

            float necessaryHeight = 32f * observers.Sum(observer => observer.Available ? 1 : 0);
            float necessaryWidth = maximumTextLength;

            if (necessaryHeight < 31.9999f)
            {
                MenuOpen = false;
                return Vector2.zero;
            }

            var old = GUI.skin;

            GUI.skin = Settings.Skin;

            GUILayout.BeginArea(new Rect(position.x, position.y, necessaryWidth, necessaryHeight));
            GUILayout.BeginVertical(GUILayout.ExpandWidth(true));

            foreach (var observer in observers)
                if (observer.Available)
                {
                    var content = new GUIContent(observer.ExperimentTitle);

                    if (GUILayout.Button(content))
                    {
                        Log.Debug("Deploying {0}", observer.ExperimentTitle);
                        observer.Deploy();
                    }
                }
            GUILayout.EndVertical();
            GUILayout.EndArea();

            GUI.skin = old;

            return new Vector2(necessaryWidth, necessaryHeight);
        }


        public void OnToolbarClick(ClickEvent ce)
        {
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
            else // right click
            {
                Log.Debug("Toolbar right clicked");

                OptionsOpen = !OptionsOpen;
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
                if (State == IconState.NewResearch) State = IconState.ResearchAvailable;
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
                return mainButton.Drawable != null && mainButton.Drawable is ExperimentIndicator;
            }
            set
            {
                mainButton.Drawable = value ? this : null;
                UpdateMenuState();
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
                }
                else if (OptionsOpen)
                {
                    mainButton.Drawable = null;
                    Settings.Instance.Save();
                }
                UpdateMenuState();
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

        private ExperimentSituations VesselSituationToExperimentSituation()
        {
            //Log.Debug("Flying low altitude <= {0}, Low space altitude <= {1}",(double)FlightGlobals.ActiveVessel.mainBody.scienceValues.flyingAltitudeThreshold,  (double)FlightGlobals.ActiveVessel.mainBody.scienceValues.spaceAltitudeThreshold);

            switch (FlightGlobals.ActiveVessel.situation)
            {
                case Vessel.Situations.LANDED:
                case Vessel.Situations.PRELAUNCH:
                    return ExperimentSituations.SrfLanded;
                case Vessel.Situations.SPLASHED:
                    return ExperimentSituations.SrfSplashed;
                case Vessel.Situations.FLYING:
                    if (FlightGlobals.ActiveVessel.altitude < (double)FlightGlobals.ActiveVessel.mainBody.scienceValues.flyingAltitudeThreshold)
                        return ExperimentSituations.FlyingLow;

                    return ExperimentSituations.FlyingHigh;

                default:
                    if (FlightGlobals.ActiveVessel.altitude < (double)FlightGlobals.ActiveVessel.mainBody.scienceValues.spaceAltitudeThreshold)

                        return ExperimentSituations.InSpaceLow;
                    return ExperimentSituations.InSpaceHigh;
            }
        }
        #endregion
    }
}
