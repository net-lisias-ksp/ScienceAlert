using System;
using ScienceAlert.Game;

namespace ScienceAlert.VesselContext.Experiments.Rules
{
    public class ExperimentIsAvailableDuringVesselSituation : IExperimentRule
    {
        private readonly ScienceExperiment _experiment;
        private readonly IVessel _vessel;

        public ExperimentIsAvailableDuringVesselSituation(
            ScienceExperiment experiment, 
            IVessel vessel)
        {
            if (experiment == null) throw new ArgumentNullException("experiment");
            if (vessel == null) throw new ArgumentNullException("vessel");
            _experiment = experiment;
            _vessel = vessel;
        }


        public bool Get()
        {
            return _experiment.IsAvailableWhile(_vessel.ExperimentSituation, _vessel.Body);
        }
    }
}
