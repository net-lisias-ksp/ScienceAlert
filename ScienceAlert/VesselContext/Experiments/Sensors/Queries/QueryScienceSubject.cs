using System;
using System.Linq;
using ScienceAlert.Game;

namespace ScienceAlert.VesselContext.Experiments.Sensors.Queries
{
    public class QueryScienceSubject : IQueryScienceSubject
    {
        private readonly ICelestialBodyProvider _bodyProvider;
        private readonly IExperimentSituationProvider _situationProvider;
        private readonly IExperimentBiomeProvider _biomeProvider;

        public QueryScienceSubject(
            ICelestialBodyProvider bodyProvider, 
            IExperimentSituationProvider situationProvider,
            IExperimentBiomeProvider biomeProvider)
        {
            if (bodyProvider == null) throw new ArgumentNullException("bodyProvider");
            if (situationProvider == null) throw new ArgumentNullException("situationProvider");
            if (biomeProvider == null) throw new ArgumentNullException("biomeProvider");
            _bodyProvider = bodyProvider;
            _situationProvider = situationProvider;
            _biomeProvider = biomeProvider;
        }


        public ScienceSubject GetSubject(ScienceExperiment experiment)
        {
            if (experiment == null) throw new ArgumentNullException("experiment");

            var subj = new ScienceSubject(
                experiment, 
                _situationProvider.ExperimentSituation,
                _bodyProvider.OrbitingBody, 
                experiment.BiomeIsRelevantWhile(_situationProvider.ExperimentSituation) ? _biomeProvider.Biome : string.Empty);

            var subj1 = subj;
            var existingSubj = ResearchAndDevelopment.GetSubjects().FirstOrDefault(ss => ss.id == subj1.id);

            if (existingSubj != null) subj = existingSubj;

            return subj;
        }
    }
}
