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
using DebugTools;

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
        protected Settings.ExperimentSettings settings;     // settings for this experiment
        protected string lastAvailableId;                   // Id of the last time the experiment was available
        protected string lastBiomeQuery;                    // the last good biome result we had
        protected BiomeFilter biomeFilter;                  // Provides a little more accuracy when it comes to determining current biome (the original biome map has some filtering done on it)

/******************************************************************************
 *                    Implementation Details
 ******************************************************************************/


        public ExperimentObserver(StorageCache cache, Settings.ExperimentSettings expSettings, BiomeFilter filter, string expid)
        {
            settings = expSettings;
            biomeFilter = filter;

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
        public virtual float GetScienceTotal(ScienceSubject subject, out List<ScienceData> data)
        {
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

                // we can consider a maximum of two reports. At least until I figure
                // out how exactly the decay factor works or do something hacky to fake it
                if (found.Count() > 1)
                {
                    // note: hacky workaround for now: just assume the third report etc
                    // has the same value as the second. It'll overestimate but that's
                    // better than underestimating and possibly not hitting the threshold
                    // that would stop the player from getting spammed with alerts
                    for (int i = 0; i < found.Count() - 1; ++i)
                        potentialScience += ResearchAndDevelopment.GetNextScienceValue(found.First().dataAmount, subject);

                    //Log.Debug("Found a second report; its approximate value is {0}", ResearchAndDevelopment.GetNextScienceValue(found.First().dataAmount, subject));
                }
                return potentialScience;
            }
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
                        if (biomeFilter.GetBiome(vessel.latitude * Mathf.Deg2Rad, vessel.longitude * Mathf.Deg2Rad, out biome))
                        {
                            lastBiomeQuery = biome;
                        }
                        else
                        {
                            biome = lastBiomeQuery; // we're almost certainly still in the old area then
                        }


                    try
                    {
                        var subject = ResearchAndDevelopment.GetExperimentSubject(experiment, experimentSituation, vessel.mainBody, biome);
                        List<ScienceData> data = null;
                        float scienceTotal = GetScienceTotal(subject, out data);
                        //Log.Debug("{0} scienceTotal = {1}, scienceCap = {2}", subject.id, scienceTotal, subject.scienceCap);

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
                        if (Available)
                        {
                            if (lastAvailableId != subject.id)
                                lastStatus = false; // force a refresh, in case we're going from available -> available in different subject id

                            lastAvailableId = subject.id;

                            if (Available != lastStatus && Available)
                            {
                                Log.Write("Experiment {0} just became available! Total potential science onboard currently: {1} (Cap is {2}, threshold is {3}, current sci is {4})", lastAvailableId, scienceTotal, subject.scienceCap, settings.Filter, subject.science);

#if DEBUG
                            if (data.Count() > 0)
                            {
                                Log.Write("Raw dataAmount = {0}, nextScience = {1}", data.First().dataAmount, ResearchAndDevelopment.GetScienceValue(data.First().dataAmount, subject));
                                Log.Write("Total science value = {0}", GetScienceTotal(subject, out data));
                            }
#endif
                            }
                        }

                    } catch (NullReferenceException e)
                    {
                        // trying to track down this, which occurs sometimes when
                        // switching vessels:
                        /*
                         * NullReferenceException: Object reference not set to an instance of an object
  at ScienceSubject..ctor (.ScienceExperiment exp, ExperimentSituations sit, .CelestialBody body, System.String biome) [0x00000] in <filename unknown>:0 
  at ResearchAndDevelopment.GetExperimentSubject (.ScienceExperiment experiment, ExperimentSituations situation, .CelestialBody body, System.String biome) [0x00000] in <filename unknown>:0 
  at ScienceAlert.ExperimentObserver.UpdateStatus (ExperimentSituations experimentSituation) [0x00000] in <filename unknown>:0 
  at ScienceAlert.ScienceAlert+<UpdateObservers>d__8.MoveNext () [0x00000] in <filename unknown>:0 
  at ScienceAlert.ScienceAlert.Update () [0x00000] in <filename unknown>:0 
                         * */
                        Log.Error("NullReferenceException: {0}", e);
                        if (experiment == null) Log.Error("Observer's experiment is null");
                        if (experimentSituation == null) Log.Error("bad situation");
                        if (vessel == null) Log.Error("Vessel is null");
                        if (vessel.mainBody == null) Log.Error("mainBody is null");
                        if (biome == null) Log.Error("biome is null");

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
                        PopupDialog.SpawnPopupDialog("Error", string.Format("Cannot deploy custom experiment {0} because it does not extend ModuleScienceExperiment; you will have to manually deploy it.  Sorry!", ExperimentTitle), "Okay", false, HighLogic.Skin);
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

        public bool StopWarpOnDiscovery
        {
            get
            {
                return settings.StopWarpOnDiscovery;
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
        public EvaReportObserver(StorageCache cache, Settings.ExperimentSettings settings, BiomeFilter filter)
            : base(cache, settings, filter, "evaReport")
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

                        options[0] = new DialogOption("Science is worth a little risk",new Callback(OnConfirmEva));
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
            Log.Write("EvaObserver: User confirmed eva despite conditions");
            Log.Write("Expelling... {0}", ExpelCrewman() ? "success!" : "failed");
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
                // 1.5 bugfix:
                //   if the player is in IVA view when we spawn eva, it looks
                // like KSP doesn't switch cameras automatically
                if ((CameraManager.Instance.currentCameraMode & (CameraManager.CameraMode.Internal | CameraManager.CameraMode.IVA)) != 0)
                {
                    Log.Write("Detected IVA or internal cam; switching to flight cam");
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
