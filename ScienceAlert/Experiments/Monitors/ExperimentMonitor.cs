using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using ReeperCommon;

namespace ScienceAlert.Experiments.Monitors
{
    using ModuleList = List<ModuleScienceExperiment>;

    public class ExperimentMonitor
    {

        // members
        private BiomeFilter filter;
        ScienceExperiment experiment;                               // which experiment this monitor watches
        ProfileData.ExperimentSettings settings;                    // This monitor's settings
        Experiments.Data.ScienceDataCache cache;                    // vessel ScienceData cache

        private ModuleList recoveryModules;                         // cache of all ModuleScienceExperiments on the Vessel, sorted
                                                                    // least-to-greatest by xmitScalar
        private ModuleList transmitModules;                         // same as above, but in reverse order

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="experiment"></param>
        /// <param name="settings"></param>
        public ExperimentMonitor(string expid, ProfileData.ExperimentSettings settings, Experiments.Data.ScienceDataCache cache, ModuleList modules)
        {
            this.settings = settings;
            this.cache = cache;
            this.filter = API.ScienceAlert.BiomeFilter;

            experiment = ResearchAndDevelopment.GetExperiment(expid);

            // this is to avoid resorting the list on every UpdateStatus
            transmitModules = modules.OrderByDescending(mse => mse.xmitDataScalar).ToList();
            recoveryModules = new ModuleList(transmitModules);
            recoveryModules.Reverse();
        }



        /// <summary>
        /// Returns current status of this experiment
        /// </summary>
        /// <param name="situation"></param>
        /// <returns></returns>
        public API.ExperimentStatus UpdateStatus(ExperimentSituations situation)
        {
            // first, let's make sure it's even possible to do
            if (!IsRunnable)
            {
                Status = API.ExperimentStatus.NoAlert;
                return Status;
            }

            // get subject representing this experiment + biome + body + situation combo
            var subject = ResearchAndDevelopment.GetExperimentSubject(experiment, situation, FlightGlobals.ActiveVessel.mainBody, experiment.BiomeIsRelevantWhile(situation) ? filter.CurrentBiome : string.Empty);
            Subject = subject.id;

                if (subject.scienceCap < 0.001f) // this might happen if somebody tried to disable an experiment by setting its max to 0
                    return API.ExperimentStatus.NoAlert;

            // stored reports will affect the next report's value so we'll be needing those
            var stored = cache.FindStoredData(subject.id);

            // calculate next recovery value
            RecoveryValue = subject.CalculateNextReport(experiment, stored, 1f);

            // calculate next transmission value, using next best available module (it's possible for two
            // ExperimentModules on a Vessel to have different xmit scalars, though unlikely)
            TransmissionValue = subject.CalculateNextReport(experiment, stored, GetNextModule(false).xmitDataScalar);


            // now we'll need totals to determine whether RecoveryValue of TransmissionValue
            // passes our filter settings (in most cases)
            // note: these totals are science the player already has or can delivery and do not
            // include the theoretical RecoveryValue or TransmissionValue from above
            float recoveryTotal = API.CalculateScienceTotal(experiment, subject, stored);
            float transmitTotal = API.CalculateScienceTotal(experiment, subject, stored,
                GetNextModule(false).xmitDataScalar);

#if DEBUG
            Log.Debug("Calculated {0} next report, {1} total for {2}", RecoveryValue, recoveryTotal, experiment.id);
            // todo: transmission
#endif
            // and now we can check if either threshold passes our alert threshold
            // recovery
            API.ExperimentStatus updatedStatus = API.ExperimentStatus.NoAlert;

            // by recovery
            if (RecoveryValue > 0.01f) // only consider recovery if the report is actually worth something
            {
                switch (settings.Filter)
                {
                    // "only alert me if I have no data whatsoever on this experiment"
                    case ProfileData.ExperimentSettings.FilterMethod.Unresearched:
                        if (recoveryTotal < 0.001f)
                            updatedStatus |= API.ExperimentStatus.Recoverable;
                        break;

                    // "alert me if this subject has less than 50% of its science gathered"
                    case ProfileData.ExperimentSettings.FilterMethod.LessThanFiftyPercent:
                        if (recoveryTotal / subject.scienceCap < 0.5f)
                                updatedStatus |= API.ExperimentStatus.Recoverable;
                        break;

                    // "alert me if this subject has less than 90% of its science gathered"
                    case ProfileData.ExperimentSettings.FilterMethod.LessThanNinetyPercent:
                        if (recoveryTotal / subject.science < 0.9f)

                                updatedStatus |= API.ExperimentStatus.Recoverable;
                        break;

                    // "alert me if I can gain ANY science"
                    case ProfileData.ExperimentSettings.FilterMethod.NotMaxed:
                        updatedStatus |= API.ExperimentStatus.Recoverable;
                        break;
                }
            }


            // by transmission
            // TODO
            if (TransmissionValue > 0.01f)
            {
                // todo settings here
            }


            // bring status up to date..
            Status = updatedStatus;

            return Status;
        }



        /// <summary>
        /// Refresh list of onboard experiment modules. We don't want to be searching the entire vessel tree
        /// unnecessarily every time we check
        /// </summary>
        //public void Rescan(ModuleList onboard)
        //{
        //    Log.Verbose("Rescanning modules for experiment {0}...", experiment.id);
        //    experimentModules.Clear();

        //    // find and sort by xmitScalar, greatest to least
        //    // this is because xmitScalar is set per-part so it's conceivable that multiple
        //    // parts, each with the same experiment(s), will result in different transmission values
        //    // 
        //    // it makes the most sense to always consider the maximum value for transmission
        //    //
        //    // note to self: this could conceivably affect how we select which experiment module to
        //    // deploy, so should we add a new button in the experiment list? hmm...
        //    experimentModules = onboard.Where(mse => mse.experimentID == experiment.id).OrderByDescending(mse => mse.xmitDataScalar).ToList();

        //    Log.Verbose("Found {0} ModuleScienceExperiments for experiment {1}", experimentModules.Count, experiment.id);
        //}





        /// <summary>
        /// Returns the next module best suited for either recovery or transmission
        /// </summary>
        /// <param name="forRecovery">True if this module will be used for recovery and so should be the worst xmitDataScalar</param>
        /// <returns></returns>
        protected virtual ModuleScienceExperiment GetNextModule(bool forRecovery = true)
        {
            return forRecovery
                ? recoveryModules.First(mse => !mse.Deployed && !mse.Inoperable)
                : transmitModules.First(mse => !mse.Deployed && !mse.Inoperable);
        }

#region properties


        /// <summary>
        /// Returns true if there's at least one module available for this experiment to run
        /// </summary>
        protected virtual bool IsRunnable
        {
            get
            {
                return recoveryModules.Any(mse => !mse.Deployed && !mse.Inoperable);
            }
        }


        public float RecoveryValue
        {
            get;
            private set;
        }

        public float TransmissionValue
        {
            get;
            private set;
        }

        public API.ExperimentStatus Status
        {
            get;
            private set;
        }

        public string Subject
        {
            get;
            private set;
        }

        public virtual ScienceExperiment Experiment
        {
            get { return experiment; }
        }

        /// <summary>
        /// Returns current profile settings associated with this monitor's experiment
        /// </summary>
        public ProfileData.ExperimentSettings Settings
        {
            get
            {
                return settings;
            }
        }

#endregion
    }
}
