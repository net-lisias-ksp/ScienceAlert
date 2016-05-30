using System.Linq;
using ScienceAlert.Game;

namespace ScienceAlert.VesselContext.Experiments.Rules
{
    // ReSharper disable once UnusedMember.Global
    public class RuleVesselHasOperableModuleScienceExperiment : ScienceExperimentModuleTracker, ISensorRule
    {
        public RuleVesselHasOperableModuleScienceExperiment(ScienceExperiment experiment, IVessel vessel)
            : base(experiment, vessel)
        {
        }


        public bool Passes()
        {
            return ExperimentModules.Any(mse => !mse.Deployed);
        }
    }
}
