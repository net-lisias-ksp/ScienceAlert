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
using UnityEngine;
using ReeperCommon;
using ScienceAlert.API;

namespace ScienceAlert.Experiments.Observers
{
    using ScienceModuleList = List<ModuleScienceExperiment>;

    /// <summary>
    /// Given an experiment, monitor conditions for the experiment.  If
    /// an experiment onboard is available and the conditions are right
    /// for the given filter, the experiment observer will indicate that
    /// the experiment is Available.
    /// </summary>
    public class ExperimentObserver
    {
        private ScienceModuleList modules;                  // all ModuleScienceExperiments onboard that represent our experiment
        protected ScienceExperiment experiment;             // The actual experiment that will be performed
        protected StorageCache storage;                     // Represents possible storage locations on the vessel
        public ProfileData.ExperimentSettings settings;     // settings for this experiment
        protected string lastAvailableId;                   // Id of the last time the experiment was available
        protected ScanInterface scanInterface;              // Determines whether we're allowed to know if an experiment is available
        protected float nextReportValue;                    // take a guess
        protected bool requireControllable;                 // Vessel needs to be controllable for the experiment to be available
        protected BiomeFilter biomeFilter;

        // events
        public ExperimentManager.ExperimentAvailableDelegate OnAvailable = delegate { };

/******************************************************************************
 *                    Implementation Details
 ******************************************************************************/


        public ExperimentObserver(StorageCache cache, BiomeFilter biomeFilter, ProfileData.ExperimentSettings expSettings, ScanInterface scanMapInterface, string expid)
        {
            settings = expSettings;
            this.biomeFilter = biomeFilter;
            requireControllable = true;

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
            Rescan();
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
        public virtual void Rescan()
        {
            Log.Debug("ExperimentObserver.Rescan - {0}", experiment.id);

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
                float potentialScience = subject.science + ResearchAndDevelopment.GetScienceValue(data.First().dataAmount, subject) * HighLogic.CurrentGame.Parameters.Career.ScienceGainMultiplier;

                if (found.Count() > 1)
                {
                    float secondReport = ResearchAndDevelopment.GetNextScienceValue(experiment.baseValue * experiment.dataScale, subject) * HighLogic.CurrentGame.Parameters.Career.ScienceGainMultiplier;

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
                return ResearchAndDevelopment.GetScienceValue(experiment.baseValue * experiment.dataScale, subject) * HighLogic.CurrentGame.Parameters.Career.ScienceGainMultiplier;

            float experimentValue = ResearchAndDevelopment.GetNextScienceValue(experiment.baseValue * experiment.dataScale, subject) * HighLogic.CurrentGame.Parameters.Career.ScienceGainMultiplier;

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

            if (!settings.Enabled ||
                (requireControllable && !FlightGlobals.ActiveVessel.IsControllable))
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
                        if (scanInterface.HaveScanData(vessel.latitude, vessel.longitude, vessel.mainBody))
                        {
                            biome = biomeFilter.GetCurrentBiome();
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
                            case ProfileData.ExperimentSettings.FilterMethod.Unresearched:
                                // Fairly straightforward: total science + potential should be zero
                                Available = scienceTotal < 0.0005f;
                                break;

                            case ProfileData.ExperimentSettings.FilterMethod.NotMaxed:
                                // <98% of science cap
                                Available = scienceTotal < subject.scienceCap * 0.98f * HighLogic.CurrentGame.Parameters.Career.ScienceGainMultiplier;
                                break;

                            case ProfileData.ExperimentSettings.FilterMethod.LessThanFiftyPercent:
                                // important note for these last two filters: we can only accurately
                                // predict science for up to two of the same reports. After that,
                                // it'll be highly overestimated
                                Available = scienceTotal < subject.scienceCap * 0.5f * HighLogic.CurrentGame.Parameters.Career.ScienceGainMultiplier;
                                break;

                            case ProfileData.ExperimentSettings.FilterMethod.LessThanNinetyPercent:
                                Available = scienceTotal < subject.scienceCap * 0.9f * HighLogic.CurrentGame.Parameters.Career.ScienceGainMultiplier;
                                break;

                            default: // this should NEVER occur, but nice to have a safety measure
                                // in place if I add a filter option and forget to add its logic
                                Log.Error("Unrecognized experiment filter!");
                                data = new List<ScienceData>();
                                break;
                        }

                        // bugfix: also ensure the experiment will generate >0 science, else
                        //         we could produce alerts for reports that won't generate any science
                        //nextReportValue = CalculateNextReportValue(subject, experimentSituation, data);
                        nextReportValue = subject.CalculateNextReport(experiment, data);
                        Available = Available && nextReportValue > 0.01f;

                        // check the science threshold
                        Available = Available && nextReportValue > ScienceAlertProfileManager.ActiveProfile.ScienceThreshold;



                        if (Available)
                        {
                            if (lastAvailableId != subject.id)
                                lastStatus = false; // force a refresh, in case we're going from available -> available in different subject id
                            
                            
                            lastAvailableId = subject.id;

                            if (Available != lastStatus && Available)
                            {
                                Log.Normal("Experiment {0} just became available! Total potential science onboard currently: {1} (Cap is {2}, threshold is {3}, current sci is {4}, expected next report value: {5})", lastAvailableId, scienceTotal, subject.scienceCap * HighLogic.CurrentGame.Parameters.Career.ScienceGainMultiplier, settings.Filter, subject.science, nextReportValue);

                                //Log.Debug("Transmission value: {0}", CalculateTransmissionValue(subject));

#if DEBUG
                                if (GetNextOnboardExperimentModule() != null)
                                    Log.Debug("Transmission value: {0}", API.Util.GetNextReportValue(subject, experiment, data, GetNextOnboardExperimentModule().xmitDataScalar));
#endif

                                if (data.Any())
                                {
                                    Log.Debug("Raw dataAmount = {0}, nextScience = {1}", data.First().dataAmount, ResearchAndDevelopment.GetScienceValue(data.First().dataAmount, subject) * HighLogic.CurrentGame.Parameters.Career.ScienceGainMultiplier);
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

            if (requireControllable && !FlightGlobals.ActiveVessel.IsControllable)
            {
                // note to self: the user should never see this error
                Log.Error("Cannot deploy '{0}' -- Vessel is not controllable.", experiment.experimentTitle);
                return false;
            }

           
            // find an unused science module and use it 
            //      note for crew reports: as long as a kerbal exists somewhere in the vessel hierarchy,
            //      crew reports are allowed from "empty" command modules as stock behaviour.  So we 
            //      needn't find a specific module to use
            var deployable = GetNextOnboardExperimentModule();

            if (deployable)
            {
                Log.Debug("Deploying experiment module on part {0}", deployable.part.name);

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
                //if (settings.AssumeOnboard)
                //{
                //    if (modules.Count == 0)
                //    {
                //        PopupDialog.SpawnPopupDialog("Error", string.Format("Cannot deploy custom experiment {0} because it does not extend ModuleScienceExperiment; you will have to manually deploy it. Sorry!", ExperimentTitle), "Okay", false, HighLogic.Skin);
                //        Log.Error("Custom experiment {0} has no modules and AssumeOnBoard flag; informed user that we cannot automatically deploy it.", ExperimentTitle);
                //        return false;
                //    }
                //}
                //else
                //{
                //    PopupDialog.SpawnPopupDialog("Error", string.Format("There are no open {0} experiments available onboard.", ExperimentTitle), "Okay", false, Settings.Skin);
                //    Log.Error("Failed to deploy experiment {0}; no more available science modules.", ExperimentTitle);
                //    return false;
                //}
            }

            // we should never reach this point if IsExperimentAvailableOnboard did
            // its job.  This would indicate we're not accounting for something about 
            // experiment states
            Log.Error("Logic problem: Did not deploy experiment but we should have been able to.  Investigate {0}", ExperimentTitle);
            return false;
        }



        #region Properties
        protected ModuleScienceExperiment GetNextOnboardExperimentModule()
        {
            if (modules == null)
            {
#if DEBUG
                Log.Error("ExperimentObserver.NextExperimentModule[{0}] - modules list is null!", experiment.id);
#endif
            }
            foreach (var module in modules)
                if (!module.Deployed && !module.Inoperable)
                    return module;

            return null;
        }

        public virtual bool IsReadyOnboard
        {
            get
            {
                return experiment.IsUnlocked() && GetNextOnboardExperimentModule() != null;
            }
        }



        public virtual bool Available
        {
            get;
            protected set;
        }



        //public virtual bool AssumeOnboard
        //{
        //    get
        //    {
        //        return settings.AssumeOnboard;
        //    }
        //}


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

        public ScienceExperiment Experiment
        {
            get
            {
                return experiment;
            }
        }

        #endregion

        #region helpers




        #endregion
    }


}
