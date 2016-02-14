using System;
using System.Collections.Generic;
using System.Linq;
using ScienceAlert.Game;

namespace ScienceAlert.VesselContext.Experiments.Rules
{
    public abstract class ScienceExperimentModuleTracker
    {
        protected readonly ScienceExperiment Experiment;
        protected readonly IVessel Vessel;
        protected List<IModuleScienceExperiment> ExperimentModules;

        private Func<IModuleScienceExperiment, bool> _moduleMatchesExperiment;
 
        public ScienceExperimentModuleTracker(
            ScienceExperiment experiment,
            IVessel vessel)
        {
            if (experiment == null) throw new ArgumentNullException("experiment");
            if (vessel == null) throw new ArgumentNullException("vessel");
            Experiment = experiment;
            Vessel = vessel;
            _moduleMatchesExperiment = IsModuleForOurExperiment;

            Vessel.Modified += VesselOnModified;
            VesselOnModified();
        }


        private void VesselOnModified()
        {
            ExperimentModules = Vessel.ScienceExperimentModules
                .Where(_moduleMatchesExperiment)
                .ToList();
        }


        private bool IsModuleForOurExperiment(IModuleScienceExperiment experiment)
        {
            return experiment.ExperimentID == Experiment.id;
        }
    }
}
