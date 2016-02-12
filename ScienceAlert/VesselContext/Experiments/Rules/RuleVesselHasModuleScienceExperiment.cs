using ScienceAlert.Game;

namespace ScienceAlert.VesselContext.Experiments.Rules
{
    public class RuleVesselHasModuleScienceExperiment : ScienceExperimentModuleTracker, IExperimentRule
    {
        public RuleVesselHasModuleScienceExperiment(ScienceExperiment experiment, IVessel vessel)
            : base(experiment, vessel)
        {
        }

        public bool Passes()
        {
            return ExperimentModules.Count > 0;
        }
    }
}
