using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Toolbar;

// TODO: manually selected transmitters ignore MagicTransmitter
//          further thought: who really does this?  will think on it more

// todo: separate science observer for surface samples like the eva one?
// todo: stop scanning when game is paused


// todo: only update experiments on vessel situation change?
//          further thought: maybe not.  It's not that expensive as is, plus
//          mod experiments might be changing data on the fly

namespace ExperimentIndicator
{
    using ExperimentObserverList = List<ExperimentObserver>;


    [KSPAddon(KSPAddon.Startup.Flight, false)]
    public class ExperimentIndicator : MonoBehaviour, IDrawable
    {
        enum IconState
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
        private System.Collections.IEnumerator starFlask;
        private System.Collections.IEnumerator rebuilder;

        private Toolbar.IButton mainButton;
        private IconState researchState = IconState.NoResearch;


        // texture/animation data
        private const float FRAME_RATE = 24f / 1000f;
        private const int FRAME_COUNT = 100;
        private int StarAnimationFrame = 0;
        private float ElapsedTime = 0f;

        private const string NORMAL_FLASK = "ExperimentIndicator/textures/flask";
        private List<string> STAR_FLASK = new List<string>();


        // audio 
        new AudioController audio = new AudioController();

        // experiment text related
        private float maximumTextLength = float.NaN;

// ----------------------------------------------------------------------------
//             --> Implementation of ExperimentIndicator <--
// ----------------------------------------------------------------------------

        public void Start()
        {
            for (int i = 0; i < FRAME_COUNT; ++i)
                STAR_FLASK.Add(NORMAL_FLASK + GetFrame(i + 1));

            GameEvents.onVesselWasModified.Add(OnVesselWasModified);
            GameEvents.onVesselChange.Add(OnVesselChanged);
            GameEvents.onVesselDestroy.Add(OnVesselDestroyed);

            mainButton = Toolbar.ToolbarManager.Instance.add("ExperimentIndicator", "PopupOpen");
            mainButton.Text = "ExpIndicator";
            mainButton.ToolTip = "Left Click to Open Experiments; Right Click for Settings";
            mainButton.TexturePath = NORMAL_FLASK;
            mainButton.OnClick += OnToolbarClick;

            // set up coroutines
            rebuilder = RebuildObserverList();
            starFlask = StarAnimation();
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
                maximumTextLength = observers.Max(observer => GUI.skin.button.CalcSize(new GUIContent(observer.ExperimentTitle)).x);
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
                if (!vesselStorage.IsBusy && watcher != null && starFlask != null)
                {
                    watcher.MoveNext();
                    starFlask.MoveNext();
                } else Log.Debug("storage is busy");
            }

            if (Input.GetKeyDown(KeyCode.S))
            {
                Log.Debug("Playing bubbles!");

                audio.PlaySound(AudioController.AvailableSounds.Bubbles);
            }

            //if (Input.GetKeyDown(KeyCode.U))
            //    vesselStorage.DumpContainerData();

            //if (Input.GetKeyDown(KeyCode.R))
            //    vesselStorage.ScheduleRebuild();
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
                if (Settings.Instance.GetExperimentSettings(expid).ScanEnabled)
                {
                    if (expid != "evaReport") // evaReport is a special case
                        observers.Add(new ExperimentObserver(vesselStorage, Settings.Instance.GetExperimentSettings(expid), expid));
                } else
                {
                    Log.Debug("Experiment {0} - Scan disabled", expid);
                }

            // evaReport is a special case.  It technically exists on any crewed
            // vessel.  That vessel won't report it normally though, unless
            // the vessel is itself an eva'ing Kerbal.  Since there are conditions
            // that would result in the experiment no longer being available 
            // (kerbal dies, user goes out on eva and switches back to ship, and
            // so on) I think it's best we separate it out into its own
            // Observer type that will account for these changes and any others
            // that might not necessarily trigger a VesselModified event
            if (Settings.Instance.GetExperimentSettings("evaReport").ScanEnabled)
                observers.Add(new EvaReportObserver(vesselStorage, Settings.Instance.GetExperimentSettings("evaReport")));

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
#if DEBUG
                    float start = Time.realtimeSinceStartup;
#endif

                    // Is exciting new research available?
                    if (observer.UpdateStatus(expSituation))
                    {
                        // if the menu is already open, we don't need to use the flashy animation
                        State = MenuOpen ? IconState.ResearchAvailable : IconState.NewResearch;
                        UpdateMenuState();
                    } else if (!observers.Any(ob => ob.Available)) {
                        // if no experiments are available, we should be looking
                        // at a starless flask in the menu.  Note that this is
                        // in an else statement because if UpdateStatus just
                        // returned true, we know there's at least one experiment
                        // available this frame
                        State = IconState.NoResearch;

                        if (MenuOpen) // nothing to be seen
                            MenuOpen = false;
                    }
#if DEBUG
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

            return new Vector2(necessaryWidth, necessaryHeight);
        }


        public void OnToolbarClick(ClickEvent ce)
        {
            // todo: left/right click
            Log.Write("Toolbar clicked");

            MenuOpen = !MenuOpen;
            UpdateMenuState();
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

        

        private System.Collections.IEnumerator StarAnimation()
        {
            IconState oldState = researchState;

            while (true)
            {
                if (oldState != State)
                {
                    // state has changed
                    oldState = State;

                    switch (State)
                    {
                        case IconState.NoResearch:
                            mainButton.TexturePath = NORMAL_FLASK;
                            break;
                        case IconState.NewResearch:
                            audio.PlaySound(AudioController.AvailableSounds.Bubbles);
                            mainButton.TexturePath = STAR_FLASK[StarAnimationFrame];
                            break;

                        case IconState.ResearchAvailable:
                            mainButton.TexturePath = STAR_FLASK[StarAnimationFrame];
                            break;
                    }
                }

                if (State == IconState.NewResearch)
                {
                    ElapsedTime += Time.smoothDeltaTime;

                    if (ElapsedTime > FRAME_RATE)
                    {
                        ElapsedTime = 0f;
                        StarAnimationFrame = (StarAnimationFrame + 1) % STAR_FLASK.Count;

#if DEBUG
            if (State != IconState.NewResearch)
                Log.Error("StarAnimation invoked with wrong research state {0}!", State);
#endif
                        mainButton.TexturePath = STAR_FLASK[StarAnimationFrame];
                    }
                }

                yield return 0;
            }
        }

        #endregion


        #region properties
        private IconState State
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

        private bool MenuOpen
        {
            get
            {
                return mainButton.Drawable != null;
            }
            set
            {
                mainButton.Drawable = value ? this : null;
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
