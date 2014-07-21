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
using ReeperCommon;

namespace ScienceAlert
{
    using ScienceModuleList = List<ModuleScienceExperiment>;


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
        public Settings.ExperimentSettings settings;        // settings for this experiment
        protected string lastAvailableId;                   // Id of the last time the experiment was available
        protected string lastBiomeQuery;                    // the last good biome result we had
        protected BiomeFilter biomeFilter;                  // Provides a little more accuracy when it comes to determining current biome (the original biome map has some filtering done on it)
        protected ScanInterface scanInterface;              // Determines whether we're allowed to know if an experiment is available
        protected float nextReportValue;                    // take a guess

/******************************************************************************
 *                    Implementation Details
 ******************************************************************************/


        public ExperimentObserver(StorageCache cache, Settings.ExperimentSettings expSettings, BiomeFilter filter, ScanInterface scanMapInterface, string expid)
        {
            settings = expSettings;
            biomeFilter = filter;

            if (scanMapInterface == null)
            {
                Log.Warning("ExperimentObserver for {0} given null scanning interface. Using default.", expid);
                scanMapInterface = new DefaultScanInterface();
            }

            scanInterface = scanMapInterface;

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
            modules = new ScienceModuleList();
            
            if (FlightGlobals.ActiveVessel == null)
                return;


            // locate all ModuleScienceExperiments that implement this
            // experiment.  By keeping track of them ourselves, we don't
            // need to bother ScienceAlert with any details of
            // the inner workings of this object
            ScienceModuleList potentials = FlightGlobals.ActiveVessel.FindPartModulesImplementing<ModuleScienceExperiment>();

            foreach (var potential in potentials)
                if (potential.experimentID == experiment.id)
                    modules.Add(potential);

            Log.Debug("Rebuilt ExperimentObserver for experiment {0} (active vessel has {1} experiments of type)", experiment.id, modules.Count);
        }



        /// <summary>
        /// Calculate the total science value of "known" science plus
        /// potential science for a given ScienceSubject
        /// </summary>
        /// <param name="subject"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        protected virtual float GetScienceTotal(ScienceSubject subject, out List<ScienceData> data)
        {
            if (subject == null)
            {
                Log.Error("GetScienceTotal: subject is null; cannot locate stored data");
                data = new List<ScienceData>();
                return 0f;
            }

            var found = storage.FindStoredData(subject.id);
            data = found;

            if (found.Count() == 0)
            {
                // straight stored data
                return subject.science;
            }
            else
            {
                // we've got at least one report we need to consider
                float potentialScience = subject.science + ResearchAndDevelopment.GetScienceValue(data.First().dataAmount, subject);

                if (found.Count() > 1)
                {
                    float secondReport = ResearchAndDevelopment.GetNextScienceValue(experiment.baseValue * experiment.dataScale, subject);

                    potentialScience += secondReport;

                    // there's some kind of interpolation that the game does for
                    // subsequent experiments. Dividing by four seems to give fairly
                    // decent estimate. It's very unlikely that the exact science value
                    // after the second report is going to matter one way or the other
                    // though, so this is a decent enough solution for now
                    if (found.Count > 2)
                        for (int i = 3; i < found.Count; ++i)
                            potentialScience += secondReport / Mathf.Pow(4f, i - 2);
                }
                return potentialScience;
            }
        }



        /// <summary>
        /// Nothing complicated about this; each planet has a particular
        /// multiplier for the various situations to increase the value 
        /// of experiments
        /// </summary>
        /// <param name="sit"></param>
        /// <returns></returns>
        protected float GetBodyScienceValueMultipler(ExperimentSituations sit)
        {
            var b = FlightGlobals.currentMainBody;

            switch (sit)
            {
                case ExperimentSituations.FlyingHigh:
                    return b.scienceValues.FlyingHighDataValue;
                case ExperimentSituations.FlyingLow:
                    return b.scienceValues.FlyingLowDataValue;
                case ExperimentSituations.InSpaceHigh:
                    return b.scienceValues.InSpaceHighDataValue;
                case ExperimentSituations.InSpaceLow:
                    return b.scienceValues.InSpaceLowDataValue;
                case ExperimentSituations.SrfLanded:
                    return b.scienceValues.LandedDataValue;
                case ExperimentSituations.SrfSplashed:
                    return b.scienceValues.SplashedDataValue;
                default:
                    return 0f;
            }

        }



        /// <summary>
        /// Calculate the value of a report if taken at this instant while
        /// considering existing science and reports stored onboard
        /// </summary>
        /// <param name="subject"></param>
        /// <param name="situation"></param>
        /// <param name="stored"></param>
        /// <returns></returns>
        protected float CalculateNextReportValue(ScienceSubject subject, ExperimentSituations situation, List<ScienceData> stored)
        {
            if (stored.Count == 0)
                return ResearchAndDevelopment.GetScienceValue(experiment.baseValue * experiment.dataScale, subject);

            float experimentValue = ResearchAndDevelopment.GetNextScienceValue(experiment.baseValue * experiment.dataScale, subject);

            if (stored.Count == 1)
                return experimentValue;


            // for two or more, we'll have to estimate. Presumably there's some
            // kind of interpolation going on. I've found that just dividing the previous
            // value by four is a good estimate.
            return experimentValue / Mathf.Pow(4f, stored.Count - 1);
        }




        /// <summary>
        /// Returns true if the status just changed to available (so that
        /// ScienceAlert can play a sound when the experiment
        /// status changes)
        /// </summary>
        /// <returns></returns>
        public virtual bool UpdateStatus(ExperimentSituations experimentSituation)
        {
            if (FlightGlobals.ActiveVessel == null)
            {
                Available = false;
                lastAvailableId = "";
                Log.Debug("Observer.UpdateStatus: active vessel is null!");
                return false;
            }

            if (!settings.Enabled)
            {
                Available = false;
                lastAvailableId = "";
                return false;
            }

            bool lastStatus = Available;
            var vessel = FlightGlobals.ActiveVessel;

            if (!storage.IsBusy && IsReadyOnboard)
            {
                // does this experiment even apply in the current situation?
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
                    {
                        // biome matters; check to make sure we have biome data available
                        if (scanInterface.HaveScanData(vessel.latitude, vessel.longitude))
                        {
                            if (biomeFilter.GetBiome(vessel.latitude * Mathf.Deg2Rad, vessel.longitude * Mathf.Deg2Rad, out biome))
                            {
                                lastBiomeQuery = biome;
                            }
                            else
                            {
                                biome = lastBiomeQuery; // use last good known value
                            }
                        }
                        else
                        { // no biome data available
                            Available = false;
                            lastAvailableId = "";
                            return false;
                        }

                    }

                    try
                    {
                        var subject = ResearchAndDevelopment.GetExperimentSubject(experiment, experimentSituation, vessel.mainBody, biome);
                        List<ScienceData> data = null;
                        float scienceTotal = GetScienceTotal(subject, out data);

                        switch (settings.Filter)
                        {
                            case Settings.ExperimentSettings.FilterMethod.Unresearched:
                                // Fairly straightforward: total science + potential should be zero
                                Available = scienceTotal < 0.0005f;
                                break;

                            case Settings.ExperimentSettings.FilterMethod.NotMaxed:
                                // <98% of science cap
                                Available = scienceTotal < subject.scienceCap * 0.98;
                                break;

                            case Settings.ExperimentSettings.FilterMethod.LessThanFiftyPercent:
                                // important note for these last two filters: we can only accurately
                                // predict science for up to two of the same reports. After that,
                                // it'll be highly overestimated
                                Available = scienceTotal < subject.scienceCap * 0.5f;
                                break;

                            case Settings.ExperimentSettings.FilterMethod.LessThanNinetyPercent:
                                Available = scienceTotal < subject.scienceCap * 0.9f;
                                break;

                            default: // this should NEVER occur, but nice to have a safety measure
                                // in place if I add a filter option and forget to add its logic
                                Log.Error("Unrecognized experiment filter!");
                                data = new List<ScienceData>();
                                break;
                        }

                        // bugfix: also ensure the experiment will generate >0 science, else
                        //         we could produce alerts for reports that won't generate any science
                        nextReportValue = CalculateNextReportValue(subject, experimentSituation, data);
                        Available = Available && nextReportValue > 0.01f;

                        // check the science threshold
                        if (Available && Settings.Instance.EnableScienceThreshold)
                        {
                            // make sure any experiment we alert on will produce at
                            // least X science, or we should ignore it even if it
                            // would otherwise match the filter
                            Log.Debug("next report value of {0}: {1}", experiment.id, CalculateNextReportValue(subject, experimentSituation, data));


                            Available = Available && nextReportValue >= Settings.Instance.ScienceThreshold;

                            if (CalculateNextReportValue(subject, experimentSituation, data) < Settings.Instance.ScienceThreshold)
                                Log.Verbose("Experiment {0} does not meet threshold of {1}; next report value is {2}", experiment.id, Settings.Instance.ScienceThreshold, CalculateNextReportValue(subject, experimentSituation, data));
                        }

                        if (Available)
                        {
                            if (lastAvailableId != subject.id)
                                lastStatus = false; // force a refresh, in case we're going from available -> available in different subject id

                            lastAvailableId = subject.id;

                            if (Available != lastStatus && Available)
                            {
                                Log.Normal("Experiment {0} just became available! Total potential science onboard currently: {1} (Cap is {2}, threshold is {3}, current sci is {4}, expected next report value: {5})", lastAvailableId, scienceTotal, subject.scienceCap, settings.Filter, subject.science, nextReportValue);

                                if (data.Count() > 0)
                                {
                                    Log.Debug("Raw dataAmount = {0}, nextScience = {1}", data.First().dataAmount, ResearchAndDevelopment.GetScienceValue(data.First().dataAmount, subject));
                                    Log.Debug("Total science value = {0}", GetScienceTotal(subject, out data));
                                }
                            }
                        }

                    } catch (NullReferenceException e)
                    {
                        // note to self: even with all the logging I did when
                        // I could sometimes reproduce the exception, nothing
                        // looked wrong except that biome string becomes null

                        Log.Error("Failed to create {0} ScienceSubject. If you can manage to reproduce this error, let me know.", experiment.id);
                        Log.Error("Exception was: {0}", e);
                        Available = lastStatus;
                    }
                }
                else
                {
                    // experiment isn't available under this situation
#if DEBUG
                    //if (GetNextOnboardExperimentModule())
                    //Log.Verbose("{0} is onboard but not applicable in this situation {1} (vessel situation {2})", ExperimentTitle, experimentSituation, vessel.situation);
#endif
                    Available = false;
                }
            }
            else Available = false; // no experiments ready

            return Available != lastStatus && Available;
        }



        /// <summary>
        /// No secret here, run the associated experiment using
        /// the next available science part
        /// </summary>
        /// <returns></returns>
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

                try
                {
                    // Get the most-derived type and use its DeployExperiment so we don't
                    // skip any plugin-derived versions
                    deployable.GetType().InvokeMember("DeployExperiment", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.IgnoreReturn | System.Reflection.BindingFlags.InvokeMethod, null, deployable, null);
                } catch (Exception e)
                {
                    Log.Error("Failed to invoke \"DeployExperiment\" using GetType(), falling back to base type after encountering exception {0}", e);
                    deployable.DeployExperiment();
                }

                return true;
            }
            else
            {
                if (settings.AssumeOnboard)
                {
                    if (modules.Count == 0)
                    {
                        PopupDialog.SpawnPopupDialog("Error", string.Format("Cannot deploy custom experiment {0} because it does not extend ModuleScienceExperiment; you will have to manually deploy it. Sorry!", ExperimentTitle), "Okay", false, HighLogic.Skin);
                        Log.Error("Custom experiment {0} has no modules and AssumeOnBoard flag; informed user that we cannot automatically deploy it.", ExperimentTitle);
                        return false;
                    }
                }
                else
                {
                    PopupDialog.SpawnPopupDialog("Error", string.Format("There are no open {0} experiments available onboard.", ExperimentTitle), "Okay", false, Settings.Skin);
                    Log.Error("Failed to deploy experiment {0}; no more available science modules.", ExperimentTitle);
                    return false;
                }
            }

            // we should never reach this point if IsExperimentAvailableOnboard did
            // its job.  This would indicate we're not accounting for something about 
            // experiment states
            Log.Error("Logic problem: Did not deploy experiment but we should have been able to.  Investigate {0}", ExperimentTitle);
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

        public bool StopWarpOnDiscovery
        {
            get
            {
                return settings.StopWarpOnDiscovery;
            }
        }

        public float NextReportValue
        {
            get
            {
                return nextReportValue;
            }
            private set
            {
                nextReportValue = value;
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
    /// experiments though (unless the vessel is a kerbalEva part iself), 
    /// so we'll be needing a special case to handle it.
    /// 
    /// To prevent duplicate reports, we take into account any stored experiment
    /// data as normal.
    /// </summary>
    internal class EvaReportObserver : ExperimentObserver
    {
        List<Part> crewableParts = new List<Part>();

        /// <summary>
        /// Constructor
        /// </summary>
        public EvaReportObserver(StorageCache cache, Settings.ExperimentSettings settings, BiomeFilter filter, ScanInterface scanInterface)
            : base(cache, settings, filter, scanInterface, "evaReport")
        {

        }



        /// <summary>
        /// This function will do one of two things: if the active vessel
        /// isn't an eva kerbal, it will choose a kerbal at random from
        /// the crew and send them on eva (unless the conditions outside
        /// would make it dangerous, in which case player will receive
        /// a dialog instead).
        /// 
        /// On the other hand, if the active vessel is an eva kerbal, it
        /// will deploy the experiment itself.
        /// </summary>
        /// <returns></returns>
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
                // check conditions outside
                Log.Warning("Current static pressure: {0}", FlightGlobals.getStaticPressure());

                if (FlightGlobals.getStaticPressure() > Settings.Instance.EvaAtmospherePressureWarnThreshold)
                    if (FlightGlobals.ActiveVessel.GetSrfVelocity().magnitude > Settings.Instance.EvaAtmosphereVelocityWarnThreshold)
                    {
                        Log.Debug("setting up dialog options to warn player about eva risk");

                        var options = new DialogOption[2];

                        options[0] = new DialogOption("Science is worth a little risk", new Callback(OnConfirmEva));
                        options[1] = new DialogOption("No, it would be a PR nightmare", null);

                        var dlg = new MultiOptionDialog("It looks dangerous out there. Are you sure you want to send someone out? They might lose their grip!", "Dangerous Condition Alert", Settings.Skin, options);


                        PopupDialog.SpawnPopupDialog(dlg, false, Settings.Skin);
                        return true;
                    }

                
                
                if (!ExpelCrewman())
                {
                    Log.Error("EvaReportObserver.Deploy - Did not successfully send kerbal out the airlock.  Hatch might be blocked.");
                    return false;
                }

                // todo: schedule a coroutine to wait for it to exist and pop open
                // the report?

                return true;
                
            }
            else
            {
                // The vessel is indeed a kerbalEva, so we can expect to find the
                // appropriate science module now
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



        protected void OnConfirmEva()
        {
            Log.Normal("EvaObserver: User confirmed eva despite conditions");
            Log.Normal("Expelling... {0}", ExpelCrewman() ? "success!" : "failed");
        }



        /// <summary>
        /// Toss a random kerbal out the airlock
        /// </summary>
        /// <returns></returns>
        protected virtual bool ExpelCrewman()
        {
            // You might think HighLogic.CurrentGame.CrewRoster.GetNextAvailableCrewMember
            // is a logical function to use.  Actually it's possible for it to
            // generate a crew member out of thin air and put it outside, so nope
            // 
            // luckily we can specify a particular onboard Kerbal.  We'll do so by
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
                // 1.4b bugfix for the 1.4a buxfix:
                //  if the player is in map view, SetCameraFlight won't shut it
                // down for us. Whoops
                if (MapView.MapIsEnabled)
                {
                    MapView.ExitMapView();
                }

                // 1.4a bugfix:
                //   if the player is in IVA view when we spawn eva, it looks
                // like KSP doesn't switch cameras automatically
                if ((CameraManager.Instance.currentCameraMode & (CameraManager.CameraMode.Internal | CameraManager.CameraMode.IVA)) != 0)
                {
                    Log.Normal("Detected IVA or internal cam; switching to flight cam");
                    CameraManager.Instance.SetCameraFlight();
                }


                Log.Debug("Choices of kerbal:");
                foreach (var crew in crewChoices)
                    Log.Debug(" - {0}", crew.name);

                // select a kerbal target...
                var luckyKerbal = crewChoices[UnityEngine.Random.Range(0, crewChoices.Count - 1)];
                Log.Debug("{0} is the lucky Kerbal.  Out the airlock with him!", luckyKerbal.name);

                // out he goes!
                return FlightEVA.SpawnEVA(luckyKerbal.KerbalRef);
            }
        }



        /// <summary>
        /// Note: ScienceAlert will look out for vessel changes for
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
