using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace ExperimentIndicator
{
    using ScienceModuleList = List<ModuleScienceExperiment>;
    //
    //using TransmitterList = List<IScienceDataTransmitter>;
    


    /// <summary>
    /// Given an experiment, monitor conditions for the experiment.  If
    /// an experiment onboard is available and the conditions are right
    /// for the given filter, the experiment observer will indicate that
    /// the experiment is Available.
    /// </summary>
    internal class ExperimentObserver
    {
        private ScienceModuleList modules;                  // all ModuleScienceExperiments onboard that represent our experiment
        protected ScienceExperiment experiment;             // The actual experiment that will be performed
        protected StorageCache storage;                     // Represents possible storage locations on the vessel
        protected Settings.ExperimentSettings settings;     // settings for this experiment
        protected string lastAvailableId;                   // Id of the last time the experiment was available



/******************************************************************************
 *                    Implementation Details
 ******************************************************************************/


        public ExperimentObserver(StorageCache cache, Settings.ExperimentSettings expSettings, string expid)
        {
            settings = expSettings;
#if DEBUG
            if (!settings.ScanEnabled)
                Log.Error("Experiment {0} should have been disabled!", expid);
#endif
            experiment = ResearchAndDevelopment.GetExperiment(expid);

            if (experiment == null)
                Log.Error("Failed to get experiment '{0}'", expid);

            storage = cache;
            Rebuild();
        }



        ~ExperimentObserver()
        {

        }



        /// <summary>
        /// Cache ModuleScienceExperiments so we don't have to waste time
        /// looking for them later.  Any time the vessel has changed (modified,
        /// lost a part like mystery goo can, etc), this function should be 
        /// called to keep the modules up to date.
        /// </summary>
        public virtual void Rebuild()
        {
            Log.Verbose("ExperimentObserver ({0}): rebuilding...", ExperimentTitle);
            modules = new ScienceModuleList();
            

            if (FlightGlobals.ActiveVessel == null)
                return;


            // locate all ModuleScienceExperiments that implement this
            // experiment.  By keeping track of them ourselves, we don't
            // need to bother ExperimentIndicator with any details of
            // the inner workings of this object
            ScienceModuleList potentials = FlightGlobals.ActiveVessel.FindPartModulesImplementing<ModuleScienceExperiment>();

            foreach (var potential in potentials)
                if (potential.experimentID == experiment.id)
                    modules.Add(potential);

            Log.Debug("Rebuilt ExperimentObserver for experiment {0} (active vessel has {1} experiments of type)", experiment.id, modules.Count);
        }





        



        /// <summary>
        /// Returns true if the status just changed to available (so that
        /// ExperimentIndicator can play a sound when the experiment
        /// status changes)
        /// </summary>
        /// <returns></returns>
        public virtual bool UpdateStatus(ExperimentSituations experimentSituation)
        {
            if (FlightGlobals.ActiveVessel == null)
            {
                Available = false;
                return false;
            }

            //Log.Debug("Updating status for experiment {0}", ExperimentTitle);

            bool lastStatus = Available;

            // does this experiment even apply in the current situation?
            var vessel = FlightGlobals.ActiveVessel;

            if (!storage.IsBusy && IsReadyOnboard)
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
                    ScienceData data;


                    switch (settings.Filter)
                    {
                        case Settings.ExperimentSettings.FilterMethod.Unresearched:
                            // If there's a report ready to be transmitted, the experiment
                            // is hardly "Unresearched" in this situation
                            Available = !storage.FindStoredData(subject.id, out data) && subject.science < 0.0005f;
                            //Log.Debug("    - Mode: Unresearched, result {0}, science {1}, id {2}", Available, subject.science, subject.id);
                            break;

                        case Settings.ExperimentSettings.FilterMethod.NotMaxed:
                            if (storage.FindStoredData(subject.id, out data))
                            {
                                Available = subject.science + ResearchAndDevelopment.GetNextScienceValue(data.dataAmount, subject) < subject.scienceCap;
                            }
                            else Available = subject.science < subject.scienceCap;

                            //Log.Debug("    - Mode: NotMaxed, result {0}, science {1}, id {2}", Available, subject.science, subject.id);
                            break;

                        case Settings.ExperimentSettings.FilterMethod.LessThanFiftyPercent:
                            // important note for these last two filters: we can only accurately
                            // predict the NEXT science report value.  Without knowing exactly how
                            // KSP comes up with this number, we can't accurately predict exactly
                            // how multiple reports onboard will contribute.  Rather than taking
                            // a guess, I've decided to simply only account for the "next" report
                            // and ignore any instances of multiple reports on one vessel.
                            if (storage.FindStoredData(subject.id, out data))
                            {
                                Available = subject.science + ResearchAndDevelopment.GetNextScienceValue(data.dataAmount, subject) < subject.science * 0.5f;
                            }
                            else Available = subject.science < subject.scienceCap * 0.5f;

                            break;

                        case Settings.ExperimentSettings.FilterMethod.LessThanNinetyPercent:
                            if (storage.FindStoredData(subject.id, out data))
                            {
                                Available = subject.science + ResearchAndDevelopment.GetNextScienceValue(data.dataAmount, subject) < subject.scienceCap * 0.9f;
                            }
                            else Available = subject.science < subject.scienceCap * 0.9f;
                            break;

                        default:
                            Log.Error("Unrecognized experiment filter!");
                            break;
                    }

                    if (Available)
                    {
                        if (lastAvailableId != subject.id)
                            lastStatus = false; // force a refresh, in case we're going from available -> available in different subject id

                        lastAvailableId = subject.id;
                    }
                }
                else
                {
                    // experiment isn't available under this situation
#if DEBUG
                    //if (GetNextOnboardExperimentModule())
                    //Log.Verbose("    - is onboard but not applicable in this situation {1}", ExperimentTitle, experimentSituation);
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
            else
            {
                if (settings.AssumeOnboard)
                    if (modules.Count == 0)
                    {
                        PopupDialog.SpawnPopupDialog("Error", string.Format("Cannot deploy custom experiment {0} because it does not extend ModuleScienceExperiment; you will have to manually deploy it.  Sorry!", ExperimentTitle), "Okay", false, HighLogic.Skin);
                        Log.Error("Custom experiment {0} has no modules and AssumeOnBoard flag; informed user that we cannot automatically deploy it.", ExperimentTitle);
                        return false;
                    }
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
                return settings.AssumeOnboard || GetNextOnboardExperimentModule() != null;
            }
        }



        public virtual bool Available
        {
            get;
            protected set;
        }

        public virtual bool AssumeOnboard
        {
            get
            {
                return settings.AssumeOnboard;
            }
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

        public bool SoundOnDiscovery
        {
            get
            {
                return settings.SoundOnDiscovery;
            }
        }

        public bool AnimateOnDiscovery
        {
            get
            {
                return settings.AnimationOnDiscovery;
            }
        }

        #endregion

        #region helpers


        #endregion
    }



    /// <summary>
    /// Eva report is a special kind of experiment.  As long as a Kerbal
    /// is aboard the active vessel, it's "available".  A ModuleScienceExperiment
    /// won't appear in the way the other science modules do for other 
    /// experiments though, so we'll be needing a special case to handle it.
    /// 
    /// To prevent duplicate reports, we take into account any stored experiment
    /// data as normal.
    /// </summary>
    internal class EvaReportObserver : ExperimentObserver
    {
        List<Part> crewableParts = new List<Part>();

        /// <summary>
        /// Constructor.  We already know exactly which kind of
        /// </summary>
        public EvaReportObserver(StorageCache cache, Settings.ExperimentSettings settings)
            : base(cache, settings, "evaReport")
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

                    // select a kerbal target...
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
                var evas = FlightGlobals.ActiveVessel.FindPartModulesImplementing<ModuleScienceExperiment>();
                foreach (var exp in evas)
                    if (!exp.Deployed && exp.experimentID == experiment.id)
                    {
                        exp.DeployExperiment();
                        break;
                    }

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
        public override bool UpdateStatus(ExperimentSituations experimentSituation)
        {
            Log.Debug("Updating status for experiment {0}", ExperimentTitle);

            bool lastStatus = Available;

            if (FlightGlobals.ActiveVessel != null)
                if (!storage.IsBusy && IsReadyOnboard)
                {
                    var vessel = FlightGlobals.ActiveVessel;

                    if (experiment.IsAvailableWhile(experimentSituation, vessel.mainBody))
                    {
                        var biome = string.Empty; // taking what we learned about biomes and situations before ;\

                        if (experiment.BiomeIsRelevantWhile(experimentSituation))
                            biome = vessel.mainBody.BiomeMap.GetAtt(vessel.latitude * Mathf.Deg2Rad, vessel.longitude * Mathf.Deg2Rad).name;

                        // there's kerbals, the experiment is runnable, everything
                        // looks good so far.  Let's just make sure there isn't already
                        // an evaReport of this kind stored
                        ScienceData data;
                        var subject = ResearchAndDevelopment.GetExperimentSubject(experiment, experimentSituation, vessel.mainBody, biome);

                        Log.Debug("Looking for stored eva report data");

                        if (storage.FindStoredData(subject.id, out data))
                        {
                            Log.Write("Found eva stored data for id {0}!", subject.id);
                            Available = false;
                        }
                        else
                        {
                            // note: normally I would make the assumption that since
                            // evaReports are always full value, we don't need to
                            // do any fancy checking.  However, the safer road is not
                            // to assume anything and proceed as we did in the base
                            // class version of this function
                            switch (settings.Filter)
                            {
                                case Settings.ExperimentSettings.FilterMethod.Unresearched:
                                    Available = subject.science < 0.0005f;
                                    break;

                                case Settings.ExperimentSettings.FilterMethod.NotMaxed:
                                    Available = subject.science < subject.scienceCap;
                                    break;

                                case Settings.ExperimentSettings.FilterMethod.LessThanFiftyPercent:
                                    Available = subject.science < subject.scienceCap * 0.5f;
                                    break;

                                case Settings.ExperimentSettings.FilterMethod.LessThanNinetyPercent:
                                    Available = subject.scienceCap < subject.scienceCap * 0.9f;
                                    break;

                                default:
                                    Log.Error("Unrecognized filter mode!");
                                    Available = false;
                                    break;
                            }
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
}
