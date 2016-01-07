using System;
using System.Collections.Generic;
using System.Linq;
using ScienceAlert.Game;

namespace ScienceAlert.VesselContext.Experiments.Rules
{
    public abstract class RuleUsesRelatedScienceModuleBase
    {
        protected readonly ScienceExperiment Experiment;
        protected readonly IVessel Vessel;
        protected List<IModuleScienceExperiment> ExperimentModules;

        public RuleUsesRelatedScienceModuleBase(
            ScienceExperiment experiment, 
            IVessel vessel)
        {
            if (experiment == null) throw new ArgumentNullException("experiment");
            if (vessel == null) throw new ArgumentNullException("vessel");
            Experiment = experiment;
            Vessel = vessel;

            Vessel.Modified += VesselOnModified;
            VesselOnModified();
        }


        private void VesselOnModified()
        {
            ExperimentModules = Vessel.ScienceExperimentModules
                .Where(mse => mse.experimentID == Experiment.id)
                .ToList();

            Log.Debug(() => GetType().FullName + " rescanning for " + Experiment.id + ": found " + ExperimentModules.Count + " modules (of " + Vessel.ScienceExperimentModules.Count + ") which match");
        }
    }
}
