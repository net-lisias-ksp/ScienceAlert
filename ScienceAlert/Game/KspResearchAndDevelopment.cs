using System;
using System.Collections.Generic;
using System.Linq;

namespace ScienceAlert.Game
{
    public class KspResearchAndDevelopment : IQueryScienceValue, IResearchAndDevelopment, IExistingScienceSubjectProvider
    {
        private readonly IGameFactory _gameFactory;

        public KspResearchAndDevelopment(IGameFactory gameFactory)
        {
            if (gameFactory == null) throw new ArgumentNullException("gameFactory");
            _gameFactory = gameFactory;
        }


        public float GetScienceValue(float dataAmount, IScienceSubject subject, float xmitScalar)
        {
            if (subject == null) throw new ArgumentNullException("subject");
            if (dataAmount < 0f) throw new ArgumentException("Negative data amount", "dataAmount");
            if (xmitScalar < 0f) throw new ArgumentException("Negative transmission multiplier", "xmitScalar");

            return ResearchAndDevelopment.GetScienceValue(dataAmount, subject.Subject, xmitScalar);
        }


        public float GetNextScienceValue(float dataAmount, IScienceSubject subject, float xmitScalar)
        {
            if (subject == null) throw new ArgumentNullException("subject");
            if (dataAmount < 0f) throw new ArgumentException("Negative data amount", "dataAmount");
            if (xmitScalar < 0f) throw new ArgumentException("Negative transmission multiplier", "xmitScalar");

            return ResearchAndDevelopment.GetNextScienceValue(dataAmount, subject.Subject, xmitScalar);
        }


        public float GetReferenceDataValue(float dataAmount, IScienceSubject subject)
        {
            if (subject == null) throw new ArgumentNullException("subject");
            if (dataAmount < 0f) throw new ArgumentException("Negative data amount", "dataAmount");

            return ResearchAndDevelopment.GetReferenceDataValue(dataAmount, subject.Subject);
        }


        public List<IScienceSubject> GetSubjects()
        {
            return ResearchAndDevelopment.GetSubjects().Select(sub => _gameFactory.Create(sub)).ToList();
        }


        // todo: this is producing too much garbage, maybe use cached subjects?
        public IScienceSubject GetExistingSubject(ScienceExperiment experiment, ExperimentSituations situation, ICelestialBody body, string biome)
        {
            var currentBiome = string.Empty;

            if (experiment.BiomeIsRelevantWhile(situation)) currentBiome = biome;

            var newSubject = new ScienceSubject(experiment, situation, body.Body,
                currentBiome);

            var existingSubject = GetSubjects().FirstOrDefault(s => s.Id == newSubject.id);

            return existingSubject ?? _gameFactory.Create(newSubject);
        }
    }
}
