using System;
using System.Collections.Generic;
using System.Linq;
using ScienceAlert.Game;

namespace ScienceAlert.VesselContext.Experiments.Sensors.Rules
{
    /// <summary>
    /// Used as a base for objects that need to keep track of science modules for a particular experiment
    /// </summary>
    public abstract class ScienceExperimentModuleTracker
    {
        protected readonly ScienceExperiment Experiment;
        protected readonly IVessel Vessel;
        protected readonly List<IModuleScienceExperiment> ExperimentModules = new List<IModuleScienceExperiment>();

        protected ScienceExperimentModuleTracker(
            ScienceExperiment experiment,
            IVessel vessel)
        {
            if (experiment == null) throw new ArgumentNullException("experiment");
            if (vessel == null) throw new ArgumentNullException("vessel");

            Experiment = experiment;
            Vessel = vessel;

            Vessel.Rescanned += VesselOnRescanned;
            VesselOnRescanned();
        }


        private void VesselOnRescanned()
        {
            ExperimentModules.Clear();

            foreach (var m in Vessel.ScienceExperimentModules)
                if (IsModuleForOurExperiment(m))
                    ExperimentModules.Add(m);
        }


        private bool IsModuleForOurExperiment(IModuleScienceExperiment experiment)
        {
            return experiment.ExperimentID == Experiment.id;
        }
    }
}
