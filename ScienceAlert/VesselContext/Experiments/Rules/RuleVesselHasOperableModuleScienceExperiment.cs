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
            // ReSharper disable once LoopCanBeConvertedToQuery
            foreach (var module in ExperimentModules)
                if (!module.Deployed)
                    return true;
            return false;
        }
    }
}
