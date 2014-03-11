using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Toolbar;

// TODO: manually selected transmitters ignore MagicTransmitter
// TODO: option setting to enable/disable session-persistent maneuver nodes from eva kerbal
// todo: separate science observer for surface samples like the eva one?

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

        private Toolbar.IButton mainButton;
        private IconState researchState = IconState.NoResearch;


        // texture/animation data
        private const float FRAME_RATE = 24f / 1000f;
        private const int FRAME_COUNT = 100;
        private int StarAnimationFrame = 0;

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

            StartCoroutine(RebuildObserverList());

            // run update loop
            InvokeRepeating("UpdateObservers", 1f, 1f);
        }


        public void OnDestroy()
        {
            Log.Debug("ExperimentIndicator destroyed");
            mainButton.Destroy();

        }

        public void OnGUI()
        {
            if (float.IsNaN(maximumTextLength) && observers.Count > 0)
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
            // empty

            if (Input.GetKeyDown(KeyCode.S))
            {
                Log.Debug("Playing bubbles!");

                audio.PlaySound(AudioController.AvailableSounds.Bubbles);
            }

            if (Input.GetKeyDown(KeyCode.U))
                vesselStorage.DumpContainerData();

            if (Input.GetKeyDown(KeyCode.R))
                vesselStorage.ScheduleRebuild();
        }

        
        

        public void OnVesselWasModified(Vessel vessel)
        {
            if (vessel == FlightGlobals.ActiveVessel)
            {
                Log.Debug("OnVesselWasModified invoked, rebuilding observer list");
                observers.Clear();

                try
                {
                    StopCoroutine("RebuildObserverList");
                }
                catch { }

                StartCoroutine("RebuildObserverList");
            }
        }

        public void OnVesselChanged(Vessel newVessel)
        {
            try
            {
                StopCoroutine("RebuildObserverList");
            } catch (NullReferenceException) {}

            Log.Debug("OnVesselChange: {0}", newVessel.name);
            observers.Clear();

            try
            {
                StartCoroutine("RebuildObserverList");
            } catch (NullReferenceException e)
            {
                Log.Error("Null reference exception in OnVesselChanged; did not rebuild observer list.  Exception = {0}", e);
            }
        }

        public void OnVesselDestroyed(Vessel vessel)
        {
            if (FlightGlobals.ActiveVessel == vessel)
            {
                Log.Debug("Active vessel was destroyed!");
                observers.Clear();
            }
        }

        private System.Collections.IEnumerator RebuildObserverList()
        {
            observers.Clear();
            maximumTextLength = float.NaN;

            while (ResearchAndDevelopment.Instance == null || !FlightGlobals.ready || FlightGlobals.ActiveVessel.packed)
                yield return 0;

            if (vesselStorage == null)
                vesselStorage = gameObject.AddComponent<StorageCache>();

            // construct the experiment observer list ...
            foreach (var expid in ResearchAndDevelopment.GetExperimentIDs())
                if (expid != "evaReport")
                    observers.Add(new ExperimentObserver(vesselStorage, expid));

            // evaReport is a special case.  It technically exists on any crewed
            // vessel.  That vessel won't report it normally though, unless
            // the vessel is itself an eva'ing Kerbal.  Since there are conditions
            // that would result in the experiment no longer being available 
            // (kerbal dies, user goes out on eva and switches back to ship, and
            // so on) I think it's best we separate it out into its own
            // Observer type that will account for these changes and any others
            // that might not necessarily trigger a VesselModified event
            observers.Add(new EvaReportObserver(vesselStorage));
        }





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

        public void UpdateObservers()
        {
            if (FlightGlobals.ActiveVessel == null)
                return;

#if DEBUG
            float start = Time.realtimeSinceStartup;
#endif

            // if any new experiments become available, our state
            // changes (remember: observers return true only if their observed
            // experiment wasn't available before and just become available this update)
            var expSituation = VesselSituationToExperimentSituation();

            bool stateChange = observers.Any(observer => observer.UpdateStatus(expSituation));

            // Is exciting new research available?
            if (stateChange)
            {
                // if the menu is already open, we don't need to use the flashy animation
                State = MenuOpen ? IconState.ResearchAvailable : IconState.NewResearch;
            }
            else
            {
                // if stateChange is true, we know at least one experiment is
                // available.  On the other hand, if it's false we don't know if
                // any experiments are available.  If there aren't any,
                // the icon should be the normal starless flask
                if (!observers.Any(observer => observer.Available))
                {
                    State = IconState.NoResearch;

                    if (MenuOpen) // nothing to be seen
                        MenuOpen = false;
                }
            }

            UpdateMenuState();

#if DEBUG
            Log.Warning("Tick time: {0} ms", (Time.realtimeSinceStartup - start) * 1000f);
#endif
        }

        public void StarAnimation()
        {
            StarAnimationFrame = (StarAnimationFrame + 1) % STAR_FLASK.Count;

#if DEBUG
            if (State != IconState.NewResearch)
                Log.Error("StarAnimation invoked with wrong research state {0}!", State);
#endif
            mainButton.TexturePath = STAR_FLASK[StarAnimationFrame];
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
                IconState oldState = researchState;
                researchState = value;

                switch (researchState)
                {
                    case IconState.NoResearch: // standard flask icon
                        mainButton.TexturePath = NORMAL_FLASK;

                        if (oldState == IconState.NewResearch)
                        {
                            // cancel animation function
                            CancelInvoke("StarAnimation");
                        }
                        break;

                    case IconState.ResearchAvailable: // star flask icon, no animation
                        mainButton.TexturePath = STAR_FLASK[0];

                        if (oldState == IconState.NewResearch)
                        {
                            // cancel animation function
                            CancelInvoke("StarAnimation");
                        }
                        break;

                    case IconState.NewResearch: // star flask icon plus animation
                        if (oldState != State)
                        {    // start animation function, otherwise it's already running
                            InvokeRepeating("StarAnimation", 0f, FRAME_RATE);
                            audio.PlaySound(AudioController.AvailableSounds.Bubbles);
                        }
                        break;
                }
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
