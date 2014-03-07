using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Toolbar;


// TODO: exclude any experiments for which data is stored in ModuleScienceContainer?
// todo: separate science observer for surface samples like the eva one?

namespace ExperimentIndicator
{
    using ScienceModuleList = List<ModuleScienceExperiment>;
    using ExperimentObserverList = List<ExperimentObserver>;


    /// <summary>
    /// Given an experiment, monitor conditions for the experiment.  If
    /// an experiment onboard is available and the conditions are right
    /// for the given filter, the experiment observer will indicate that
    /// the experiment is Available.
    /// </summary>
    internal class ExperimentObserver
    {
        private ScienceModuleList modules;              // all ModuleScienceExperiments onboard that represent our experiment
        protected ScienceExperiment experiment;           // The actual experiment that will be performed
        
        public enum FilterMode
        {
            Unresearched = 0,                           // only light on subjects for which no science has been confirmed at all
            NotMaxed = 1,                               // light whenever the experiment subject isn't maxed
        }


        public ExperimentObserver(string expid)
        {
            experiment = ResearchAndDevelopment.GetExperiment(expid);
            Rebuild();
        }

        ~ExperimentObserver()
        {

        }


        public virtual void Rebuild()
        {
            Log.Verbose("ExperimentObserver ({0}): rebuilding...", ExperimentTitle);
            modules = new ScienceModuleList();

            if (FlightGlobals.ActiveVessel == null)
                return;

            // todo: set filter on creation?
            Filter = FilterMode.Unresearched;

            // locate all ModuleScienceExperiments that implement this
            // experiment.  By keeping track of them ourselves, we don't
            // need to bother ExperimentIndicator with any details of
            // the inner workings of this object
            ScienceModuleList potentials = FlightGlobals.ActiveVessel.FindPartModulesImplementing<ModuleScienceExperiment>();

            foreach (var potential in potentials)
                if (potential.experimentID == experiment.id)
                    modules.Add(potential);

            Log.Debug("Added ExperimentLight for experiment {0} (active vessel has {1} experiments of type)", experiment.id, modules.Count);
        }


        /// <summary>
        /// Returns true if the status just changed to available (so that
        /// ExperimentIndicator can play a sound when the experiment
        /// status changes)
        /// </summary>
        /// <returns></returns>
        public virtual bool UpdateStatus()
        {
            if (FlightGlobals.ActiveVessel == null)
            {
                Available = false;
                return false;
            }

            Log.Debug("Updating status for experiment {0}", ExperimentTitle);

            bool lastStatus = Available;

            // does this experiment even apply in the current situation?
            var vessel = FlightGlobals.ActiveVessel;
            var experimentSituation = VesselSituationToExperimentSituation(vessel.situation);

            if (IsReadyOnboard)
            {
                if (experiment.IsAvailableWhile(experimentSituation, vessel.mainBody))
                {
                    var biome = string.Empty;

                    // note: apparently simply providing the biome name whether its
                    // relevant or not will result in the biome being INCORRECTLY applied
                    // to the experiment id.  This causes all kinds of confusion because
                    // R&D will report incorrect science values based on the wrong id
                    //
                    // Supplying an empty string if the biome doesn't matter seems to work
                    if (experiment.BiomeIsRelevantWhile(experimentSituation))
                        biome = vessel.mainBody.BiomeMap.GetAtt(vessel.latitude * Mathf.Deg2Rad, vessel.longitude * Mathf.Deg2Rad).name;

                    var subject = ResearchAndDevelopment.GetExperimentSubject(experiment, experimentSituation, vessel.mainBody, biome);

                    switch (Filter)
                    {
                        case FilterMode.Unresearched:
                            Available = subject.science < 0.0005f;
                            Log.Debug("    - Mode: Unresearched, result {0}, science {1}, id {2}", Available, subject.science, subject.id);
                            break;

                        case FilterMode.NotMaxed:
                            Available = subject.science < subject.scienceCap;
                            Log.Debug("    - Mode: NotMaxed, result {0}, science {1}, id {2}", Available, subject.science, subject.id);
                            break;
                    }

                }
                else
                {
                    // experiment isn't available under this situation
#if DEBUG
                    if (GetNextOnboardExperimentModule())
                        Log.Verbose("    - is onboard but not applicable in this situation {1}", ExperimentTitle, experimentSituation);
#endif
                    Available = false;
                }
            }
            else Available = false; // no experiments ready
            
            return Available != lastStatus && Available;
        }


        public virtual bool Deploy()
        {
            if (!Available)
            {
                Log.Error("Cannot deploy experiment {0}; Available = {1}", Available);
                return false;
            }

            if (FlightGlobals.ActiveVessel == null)
            {
                Log.Error("Deploy -- invalid active vessel");
                return false;
            }

            // find an unused science module and use it 
            //      note for crew reports: as long as a kerbal exists somewhere in the vessel hierarchy,
            //      crew reports are allowed from "empty" command modules as stock behaviour.  So we 
            //      needn't find a specific module to use
            var deployable = GetNextOnboardExperimentModule();

            if (deployable)
            {
                Log.Debug("Deploying experiment module on part {0}", deployable.part.ConstructID);
                deployable.DeployExperiment();
                return true;
            }

            // we should never reach this point if IsExperimentAvailableOnboard did
            // its job.  This would indicate we're not accounting for something about 
            // experiment states
            Log.Error("Logic problem: Did not deploy experiment, but we should have been able to.  Investigate {0}", ExperimentTitle);
            return false;
        }


        #region Properties
        private ModuleScienceExperiment GetNextOnboardExperimentModule()
        {
            foreach (var module in modules)
                if (!module.Deployed && !module.Inoperable)
                    return module;

            return null;
            
        }

        public virtual bool IsReadyOnboard
        {
            get
            {
                return GetNextOnboardExperimentModule() != null;
            }
        }



        public virtual bool Available
        {
            get;
            protected set;
        }



        public FilterMode Filter
        {
            get;
            set;
        }

        public string ExperimentTitle
        {
            get
            {
                return experiment.experimentTitle;
            }
        }

        public virtual int OnboardExperimentCount
        {
            get
            {
                return modules.Count;
            }
        }

        #endregion

        #region helpers

        protected ExperimentSituations VesselSituationToExperimentSituation(Vessel.Situations vesselSituation)
        {
            var vessel = FlightGlobals.ActiveVessel;

            switch (vesselSituation)
            {
                case Vessel.Situations.LANDED:
                case Vessel.Situations.PRELAUNCH:
                    return ExperimentSituations.SrfLanded;
                case Vessel.Situations.SPLASHED:
                    return ExperimentSituations.SrfSplashed;
                case Vessel.Situations.FLYING:
                    if (vessel.altitude < (double)vessel.mainBody.scienceValues.flyingAltitudeThreshold)
                        return ExperimentSituations.FlyingLow;

                    return ExperimentSituations.FlyingHigh;

                default:
                    if (vessel.altitude < (double)vessel.mainBody.scienceValues.spaceAltitudeThreshold)

                        return ExperimentSituations.InSpaceLow;
                    return ExperimentSituations.InSpaceHigh;
            }
        }

        #endregion
    }



    /// <summary>
    /// Eva report is a special kind of experiment.  As long as a Kerbal
    /// is aboard the active vessel, it's "available".  A ModuleScienceExperiment
    /// won't appear in the way the other science modules do for other 
    /// experiments though, so we'll be needing a special case to handle it.
    /// </summary>
    internal class EvaReportObserver : ExperimentObserver
    {
        List<Part> crewableParts = new List<Part>();

        /// <summary>
        /// Constructor.  We already know exactly which kind of
        /// </summary>
        public EvaReportObserver()
            : base("evaReport")
        {

        }

        public override bool Deploy()
        {
            if (!Available || !IsReadyOnboard)
            {
                Log.Error("Cannot deploy eva experiment {0}; Available = {1}, Onboard = {2}", Available, IsReadyOnboard);
                return false;
            }

            if (FlightGlobals.ActiveVessel == null)
            {
                Log.Error("Deploy -- invalid active vessel");
                return false;
            }


            // the current vessel IS NOT an eva'ing Kerbal, so
            // find a kerbal and dump him into space
            if (!FlightGlobals.ActiveVessel.isEVA)
            {
                // You might think HighLogic.CurrentGame.CrewRoster.GetNextAvailableCrewMember
                // is a logical function to use.  Actually it's possible for it to
                // generate a crew member out of thin air and put it outside, so nope
                // 
                // luckily we can choose a specific Kerbal.  We'll do so by
                // finding the possibilities and then picking one totally at 
                // pseudorandom

                List<ProtoCrewMember> crewChoices = new List<ProtoCrewMember>();

                foreach (var crewable in crewableParts)
                    crewChoices.AddRange(crewable.protoModuleCrew);

                if (crewChoices.Count == 0)
                {
                    Log.Error("EvaReportObserver.Deploy - No crew choices available.  Check logic");
                    return false;
                }
                else
                {
                    Log.Debug("Choices of kerbal:");
                    foreach (var crew in crewChoices)
                        Log.Debug(" - {0}", crew.name);

                    var luckyKerbal = crewChoices[UnityEngine.Random.Range(0, crewChoices.Count - 1)];
                    Log.Debug("{0} is the lucky Kerbal.  Out the airlock with him!", luckyKerbal.name);

                    // out he goes!
                    bool success = FlightEVA.SpawnEVA(luckyKerbal.KerbalRef);

                    if (!success)
                    {
                        Log.Error("EvaReportObserver.Deploy - Did not successfully send {0} out the airlock.  Hatch might be blocked.", luckyKerbal.name);
                        return false;
                    }

                    // todo: schedule a coroutine to wait for it to exist and pop open
                    // the report?

                    return true;
                }
            }
            else
            {
                Log.Error("Um ... The vessel is already an EVA kerbal.  Don't eat glue");
                return true;
            }
        }



        /// <summary>
        /// Note: ExperimentIndicator will look out for vessel changes for
        /// us and call Rebuild() as necessary
        /// </summary>
        public override void Rebuild()
        {
            crewableParts.Clear();

            if (FlightGlobals.ActiveVessel == null)
            {
                Log.Debug("EvaReportObserver: active vessel null; observer will not function");
                return;
            }

            // cache any part that can hold crew, so we don't have to
            // wastefully go through the entire vessel part tree
            // when updating status
            foreach (var part in FlightGlobals.ActiveVessel.Parts)
                if (part.CrewCapacity > 0)
                    crewableParts.Add(part);
        }



        /// <summary>
        /// The original UpdateStatus relies on ModuleScienceExperiments.
        /// If the active vessel isn't a kerbal on EVA, none will exist
        /// </summary>
        /// <returns></returns>
        public override bool UpdateStatus()
        {
            Log.Debug("Updating status for experiment {0}", ExperimentTitle);

            bool lastStatus = Available;

            if (FlightGlobals.ActiveVessel != null)
                if (IsReadyOnboard)
                {
                    var vessel = FlightGlobals.ActiveVessel;
                    var experimentSituation = VesselSituationToExperimentSituation(vessel.situation);

                    if (experiment.IsAvailableWhile(experimentSituation, vessel.mainBody))
                    {
                        var biome = string.Empty; // taking what we learned about biomes and situations before ;\

                        if (experiment.BiomeIsRelevantWhile(experimentSituation))
                            biome = vessel.mainBody.BiomeMap.GetAtt(vessel.latitude * Mathf.Deg2Rad, vessel.longitude * Mathf.Deg2Rad).name;

                        // there's kerbals, the experiment is runnable, everything
                        // looks good.  We need merely to test against the filter
                        var subject = ResearchAndDevelopment.GetExperimentSubject(experiment, experimentSituation, vessel.mainBody, biome);

                        switch (Filter)
                        {
                            case FilterMode.Unresearched:
                                Available = subject.science < 0.0005f;
                                break;

                            case FilterMode.NotMaxed:
                                Available = subject.science < subject.scienceCap;
                                break;

                            default:
                                Log.Error("Unrecognized filter mode!");
                                Available = false;
                                break;
                        }
                    }
                    else // I can't imagine any situation where evaReport wasn't technically allowed
                    {    // but who knows, maybe somebody is running a mod that changes it?
                        Log.Debug("Experiment {0} isn't available under experiment situation {1}", ExperimentTitle, experimentSituation);
                        Available = false;
                    }

                }
                else Available = false; // no kerbals onboard apparently


            // Only return true if the status just changed to "available"
            return Available != lastStatus && Available;
        }

        public override bool IsReadyOnboard
        {
            get
            {
                foreach (var crewable in crewableParts)
                    if (crewable.protoModuleCrew.Count > 0)
                        return true;
                return false;
            }
        }
    }



    internal class BoxDrawable : IDrawable
    {
        // random size for testing purposes
        private int width = 24;
        private int height = new System.Random().Next(200) + 50;

        public void Update()
        {
            // nothing to do
        }

        public Vector2 Draw(Vector2 position)
        {
            GUILayout.BeginArea(new Rect(position.x, position.y, width, height), GUI.skin.box);
            GUILayout.FlexibleSpace();
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            GUILayout.Label("something useful here");
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            GUILayout.FlexibleSpace();
            GUILayout.EndArea();

            return new Vector2(width, height);
        }
    }




    [KSPAddon(KSPAddon.Startup.Flight, false)]
    public class ExperimentIndicator : MonoBehaviour
    {
        ExperimentObserverList observers = new ExperimentObserverList();
        private Toolbar.IButton mainButton;

        public void Start()
        {
            GameEvents.onVesselWasModified.Add(OnVesselWasModified);
            GameEvents.onVesselChange.Add(OnVesselChanged);
            GameEvents.onVesselDestroy.Add(OnVesselDestroyed);

            mainButton = Toolbar.ToolbarManager.Instance.add("ExperimentIndicator", "PopupOpen");
            mainButton.Text = "blah";
            mainButton.ToolTip = "testTooltip";
            mainButton.TexturePath = "ExperimentIndicator/textures/flask";
            //mainButton.Drawable = new BoxDrawable();


            StartCoroutine(RebuildObserverList());

            // run update loop
            InvokeRepeating("UpdateObservers", 0f, 15f);

            RenderingManager.AddToPostDrawQueue(7, DrawFunction);

        }

        public void OnDestroy()
        {
            Log.Debug("ExperimentIndicator destroyed");
            mainButton.Destroy();

        }


        public void UpdateObservers()
        {
#if DEBUG
            float start = Time.realtimeSinceStartup;
#endif

            foreach (var observer in observers)
                if (observer.UpdateStatus())
                    Log.Verbose("Observer {0} is available!", observer.ExperimentTitle);
#if DEBUG
            Log.Warning("Tick time: {0} ms", (Time.realtimeSinceStartup - start) * 1000f);
#endif
        }

        public void OnVesselWasModified(Vessel vessel)
        {
            if (vessel == FlightGlobals.ActiveVessel)
            {
                Log.Debug("OnVesselWasModified invoked, rebuilding observer list");
                observers.Clear();
                StopCoroutine("RebuildObserverList");
                StartCoroutine(RebuildObserverList());
            }
        }

        public void OnVesselChanged(Vessel newVessel)
        {
            StopCoroutine("RebuildObserverList");

            Log.Debug("OnVesselChange: {0}", newVessel.name);
            observers.Clear();
            StartCoroutine(RebuildObserverList());
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

            while (ResearchAndDevelopment.Instance == null)
                yield return 0;


            // construct the experiment observer list ...
            foreach (var expid in ResearchAndDevelopment.GetExperimentIDs())
                if (expid != "evaReport")
                    if (expid == "crewReport") 
                    observers.Add(new ExperimentObserver(expid));

            // evaReport is a special case.  It technically exists on any crewed
            // vessel.  That vessel won't report it normally though, unless
            // the vessel is itself an eva'ing Kerbal.  Since there are conditions
            // that would result in the experiment no longer being available 
            // (kerbal dies, user goes out on eva and switches back to ship, and
            // so on) I think it's best we separate it out into its own
            // Observer type that will account for these changes and any others
            // that might not necessarily trigger a VesselModified event
            observers.Add(new EvaReportObserver());



        }


        public void OnGUI()
        {
            GUILayout.BeginArea(new Rect(50, 50, 300, 400));
            GUILayout.BeginVertical(GUILayout.ExpandWidth(true));

            foreach (var observer in observers)
            {
                if (observer.Available)
                    if (GUILayout.Button(new GUIContent(observer.ExperimentTitle)))
                    {
                        Log.Debug("Deploying {0}", observer.ExperimentTitle);
                        observer.Deploy();
                    }
            }

            GUILayout.EndVertical();
            GUILayout.EndArea();
        }

        public Vector2 Draw(Vector2 position)
        {
            return Vector2.zero; 
        }

        public void DrawFunction()
        {
            
        }
    }
}
