using System;
using System.Linq;

namespace ScienceAlert.Game
{
    public class KspScienceSubjectProvider : IScienceSubjectProvider
    {
        private readonly IExperimentSituationProvider _situationProvider;
        private readonly IExperimentBiomeProvider _biomeProvider;
        private readonly ICelestialBodyProvider _bodyProvider;
        private readonly IGameFactory _kspFactory;
        private readonly IResearchAndDevelopment _rnd;

        public KspScienceSubjectProvider( 
            IExperimentSituationProvider situationProvider,
            IExperimentBiomeProvider biomeProvider,
            ICelestialBodyProvider bodyProvider,
            IGameFactory kspFactory,
            IResearchAndDevelopment rnd
            )
        {
            if (situationProvider == null) throw new ArgumentNullException("situationProvider");
            if (biomeProvider == null) throw new ArgumentNullException("biomeProvider");
            if (bodyProvider == null) throw new ArgumentNullException("bodyProvider");
            if (kspFactory == null) throw new ArgumentNullException("kspFactory");
            if (rnd == null) throw new ArgumentNullException("rnd");

            _situationProvider = situationProvider;
            _biomeProvider = biomeProvider;
            _bodyProvider = bodyProvider;
            _kspFactory = kspFactory;
            _rnd = rnd;
        }


 
        // todo: this is producing too much garbage, maybe use cached subjects?
        public IScienceSubject GetSubject(ScienceExperiment experiment)
        {
            var currentSituation = _situationProvider.ExperimentSituation;
            var currentBiome = string.Empty;

            if (experiment.BiomeIsRelevantWhile(currentSituation)) currentBiome = _biomeProvider.Biome;

            var newSubject = new ScienceSubject(experiment, currentSituation, _bodyProvider.OrbitingBody.Body,
                currentBiome);

            var existingSubject = _rnd.GetSubjects().FirstOrDefault(s => s.Id == newSubject.id);

            return existingSubject ?? _kspFactory.Create(newSubject);
        }
    }
}
