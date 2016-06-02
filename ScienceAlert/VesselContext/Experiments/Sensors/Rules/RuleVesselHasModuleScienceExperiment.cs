using ScienceAlert.Game;

namespace ScienceAlert.VesselContext.Experiments.Sensors.Rules
{
    // ReSharper disable once UnusedMember.Global
    public class RuleVesselHasModuleScienceExperiment : ScienceExperimentModuleTracker, ISensorRule
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
