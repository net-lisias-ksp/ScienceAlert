using ScienceAlert.Game;

namespace ScienceAlert.VesselContext.Experiments.Rules
{
    public class VesselHasModuleScienceExperiment : RuleUsesRelatedScienceModuleBase, IExperimentRule
    {
        public VesselHasModuleScienceExperiment(ScienceExperiment experiment, IVessel vessel) : base(experiment, vessel)
        {
        }

        public bool Get()
        {
            return ExperimentModules.Count > 0;
        }
    }
}
