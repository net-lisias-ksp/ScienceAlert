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
        ScienceExperiment experiment;                               // which experiment this monitor watches
        ModuleList experimentModules = new ModuleList();            // cache of all ModuleScienceExperiments on the vessel for this
                                                                    // particular experiment



        /// <summary>
        /// Refresh list of onboard experiment modules. We don't want to be searching the entire vessel tree
        /// unnecessarily every time we check
        /// </summary>
        public void Rescan(ModuleList onboard)
        {
            Log.Verbose("Rescanning modules for experiment {0}...", experiment.id);
            experimentModules.Clear();

            // find and sort by xmitScalar, greatest to least
            // this is because xmitScalar is set per-part so it's conceivable that multiple
            // parts, each with the same experiment(s), will result in different transmission values
            // 
            // it makes the most sense to always consider the maximum value for transmission
            //
            // note to self: this could conceivably affect how we select which experiment module to
            // deploy, so should we add a new button in the experiment list? hmm...
            experimentModules = onboard.Where(mse => mse.experimentID == experiment.id).OrderByDescending(mse => mse.xmitDataScalar).ToList();

            Log.Verbose("Found {0} ModuleScienceExperiments for experiment {1}", experimentModules.Count, experiment.id);
        }



        protected ModuleScienceExperiment GetNextOnboardExperimentModule()
        {
            return experimentModules.SingleOrDefault(mse => !mse.Deployed && !mse.Inoperable);
        }



#region properties


        /// <summary>
        /// Returns true if there's at least one module available for this experiment to run
        /// </summary>
        public virtual bool IsRunnable
        {
            get
            {
                return experimentModules.Any(mse => !mse.Deployed && !mse.Inoperable);
            }
        }



#endregion
    }
}
