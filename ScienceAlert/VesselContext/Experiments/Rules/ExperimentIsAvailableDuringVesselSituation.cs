using System;
using ScienceAlert.Game;
using ScienceAlert.VesselContext.Experiments.Sensors.Queries;

namespace ScienceAlert.VesselContext.Experiments.Rules
{
    public class ExperimentIsAvailableDuringVesselSituation : IExperimentRule
    {
        private readonly ScienceExperiment _experiment;
        private readonly ICelestialBodyProvider _vesselBody;
        private readonly IExperimentSituationProvider _vesselSituation;

        public ExperimentIsAvailableDuringVesselSituation(
            ScienceExperiment experiment, 
            ICelestialBodyProvider vesselBody,
            IExperimentSituationProvider vesselSituation)
        {
            if (experiment == null) throw new ArgumentNullException("experiment");
            if (vesselBody == null) throw new ArgumentNullException("vesselBody");
            if (vesselSituation == null) throw new ArgumentNullException("vesselSituation");

            _experiment = experiment;
            _vesselBody = vesselBody;
            _vesselSituation = vesselSituation;
        }


        public bool Get()
        {
            return _experiment.IsAvailableWhile(_vesselSituation.ExperimentSituation, _vesselBody.OrbitingBody.Body);
        }
    }
}
