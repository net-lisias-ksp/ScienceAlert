using ScienceAlert.Game;

namespace ScienceAlert.VesselContext.Experiments.Rules
{
// ReSharper disable once UnusedMember.Global
    public class VesselHasOperableModuleScienceExperiment : ScienceExperimentModuleTracker, IExperimentRule
    {
        public VesselHasOperableModuleScienceExperiment(ScienceExperiment experiment, IVessel vessel) : base(experiment, vessel)
        {
        }


        public bool Passes()
        {
// ReSharper disable once LoopCanBeConvertedToQuery
// ReSharper disable once ForCanBeConvertedToForeach
            for (int i = 0; i < ExperimentModules.Count; ++i)
                if (!ExperimentModules[i].Deployed)
                    return true;

            return false;
        }
    }
}
