using ScienceAlert.Game;

namespace ScienceAlert.VesselContext.Experiments.Rules
{
    public class RuleExperimentIsAvailableWhileVesselSituation : ScienceExperimentModuleTracker, IExperimentRule
    {
        public RuleExperimentIsAvailableWhileVesselSituation(ScienceExperiment experiment, IVessel vessel) : base(experiment, vessel)
        {

        }

        public bool Passes()
        {
            return Experiment.IsAvailableWhile(Vessel.ExperimentSituation, Vessel.OrbitingBody.Body);
        }
    }
}
