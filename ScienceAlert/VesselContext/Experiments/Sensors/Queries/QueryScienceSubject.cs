using System;
using System.Linq;
using ScienceAlert.Game;

namespace ScienceAlert.VesselContext.Experiments.Sensors.Queries
{
// ReSharper disable once ClassNeverInstantiated.Global
    public class QueryScienceSubject : IQueryScienceSubject
    {
        private readonly ICelestialBodyProvider _bodyProvider;
        private readonly IExperimentSituationProvider _situationProvider;
        private readonly IExperimentBiomeProvider _biomeProvider;
        private readonly IGameFactory _gameFactory;

        public QueryScienceSubject(
            ICelestialBodyProvider bodyProvider, 
            IExperimentSituationProvider situationProvider,
            IExperimentBiomeProvider biomeProvider,
            IGameFactory gameFactory)
        {
            if (bodyProvider == null) throw new ArgumentNullException("bodyProvider");
            if (situationProvider == null) throw new ArgumentNullException("situationProvider");
            if (biomeProvider == null) throw new ArgumentNullException("biomeProvider");
            if (gameFactory == null) throw new ArgumentNullException("gameFactory");
            _bodyProvider = bodyProvider;
            _situationProvider = situationProvider;
            _biomeProvider = biomeProvider;
            _gameFactory = gameFactory;
        }


        public IScienceSubject GetSubject(ScienceExperiment experiment)
        {
            if (experiment == null) throw new ArgumentNullException("experiment");

            var subj = new ScienceSubject(
                experiment, 
                _situationProvider.ExperimentSituation,
                _bodyProvider.OrbitingBody.Body, 
                experiment.BiomeIsRelevantWhile(_situationProvider.ExperimentSituation) ? _biomeProvider.Biome : string.Empty);

            var subj1 = subj;
            var existingSubj = ResearchAndDevelopment.GetSubjects().FirstOrDefault(ss => ss.id == subj1.id);

            if (existingSubj != null) subj = existingSubj;

            return _gameFactory.Create(subj);
        }
    }
}
