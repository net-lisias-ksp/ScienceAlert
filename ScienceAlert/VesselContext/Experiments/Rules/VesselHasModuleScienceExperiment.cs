using ScienceAlert.Game;

namespace ScienceAlert.VesselContext.Experiments.Rules
{
    public class VesselHasModuleScienceExperiment : ScienceExperimentModuleTracker, IExperimentRule
    {
        public VesselHasModuleScienceExperiment(ScienceExperiment experiment, IVessel vessel) : base(experiment, vessel)
        {
        }

        public bool Passes()
        {
            return ExperimentModules.Count > 0;
        }
    }
}
