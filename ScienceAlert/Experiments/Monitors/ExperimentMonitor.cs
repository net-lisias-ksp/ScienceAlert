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
        private MonitorManager manager;
        ScienceExperiment experiment;                               // which experiment this monitor watches
        ProfileData.ExperimentSettings settings;                    // This monitor's settings
        Experiments.Data.ScienceDataCache cache;                    // vessel ScienceData cache
        ModuleList experimentModules;                               // cache of all ModuleScienceExperiments on the vessel for this
                                                                    // particular experiment


        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="experiment"></param>
        /// <param name="settings"></param>
        public ExperimentMonitor(string expid, ProfileData.ExperimentSettings settings, Experiments.Data.ScienceDataCache cache, ModuleList modules)
        {
            this.settings = settings;
            this.cache = cache;

            experiment = ResearchAndDevelopment.GetExperiment(expid);
            experimentModules = modules;


        }



        /// <summary>
        /// Returns current status of this experiment
        /// </summary>
        /// <param name="situation"></param>
        /// <returns></returns>
        API.ExperimentStatus UpdateStatus(ExperimentSituations situation)
        {
            // first, let's make sure it's even possible to do
            if (!IsRunnable)
                return API.ExperimentStatus.NoAlert;

            // get subject representing this experiment + biome + body + situation combo
            ScienceSubject subject;

            //if (experiment.BiomeIsRelevantWhile(situation))
                //subject = API.Sc

            return API.ExperimentStatus.NoAlert;
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








#region properties


        /// <summary>
        /// Returns true if there's at least one module available for this experiment to run
        /// </summary>
        protected virtual bool IsRunnable
        {
            get
            {
                return experimentModules.Any(mse => !mse.Deployed && !mse.Inoperable);
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


        protected ModuleScienceExperiment GetNextOnboardExperimentModule()
        {
            return experimentModules.SingleOrDefault(mse => !mse.Deployed && !mse.Inoperable);
        }
#endregion
    }
}
