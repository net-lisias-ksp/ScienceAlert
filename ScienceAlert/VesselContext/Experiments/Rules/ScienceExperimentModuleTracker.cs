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

        private readonly Func<IModuleScienceExperiment, bool> _moduleMatchesExperiment;

        protected ScienceExperimentModuleTracker(
            ScienceExperiment experiment,
            IVessel vessel)
        {
            if (experiment == null) throw new ArgumentNullException("experiment");
            if (vessel == null) throw new ArgumentNullException("vessel");
            Experiment = experiment;
            Vessel = vessel;
            _moduleMatchesExperiment = IsModuleForOurExperiment;

            Vessel.Rescanned += VesselOnRescanned;
            VesselOnRescanned();
        }


        private void VesselOnRescanned()
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
