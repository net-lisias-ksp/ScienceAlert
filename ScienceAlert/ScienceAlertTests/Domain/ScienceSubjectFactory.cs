using System;
using System.Collections.Generic;
using NSubstitute;
using ScienceAlert.Game;
using ScienceAlertTests.Game;

namespace ScienceAlertTests.Domain
{
    public class ScienceSubjectFactory
    {
        private static readonly Dictionary<string, CelestialBodyScienceParams> CelestialBodyScienceParams =
            new Dictionary<string, CelestialBodyScienceParams>
            {
                {
                    "Kerbin", new CelestialBodyScienceParams
                    {
                        LandedDataValue = 0.3f,
                        SplashedDataValue = 0.4f,
                        FlyingLowDataValue = 0.7f,
                        FlyingHighDataValue = 0.9f,
                        InSpaceLowDataValue = 1f,
                        InSpaceHighDataValue = 1.5f,
                        RecoveryValue = 1f,
                        flyingAltitudeThreshold = 18000,
                        spaceAltitudeThreshold = 250000,
                    }
                },
                {
                    "Duna", new CelestialBodyScienceParams
                    {
                        LandedDataValue = 8f,
                        SplashedDataValue = 1f,
                        FlyingLowDataValue = 5f,
                        FlyingHighDataValue = 5f,
                        InSpaceLowDataValue = 7f,
                        InSpaceHighDataValue = 5f,
                        RecoveryValue = 5f,
                        flyingAltitudeThreshold = 12000,
                        spaceAltitudeThreshold = 140000
                    }
                },
                {
                    "Laythe", new CelestialBodyScienceParams
                    {
                        LandedDataValue = 14,
                        SplashedDataValue = 12,
                        FlyingLowDataValue = 11,
                        FlyingHighDataValue = 10,
                        InSpaceLowDataValue = 9,
                        InSpaceHighDataValue = 8,
                        flyingAltitudeThreshold = 10000,
                        spaceAltitudeThreshold = 200000
                    }
                }
            };

        public IScienceSubject Create(ScienceExperiment experiment, string bodyName, ExperimentSituations situation,
            string subjectId, float alreadyCollectedScience)
        {
            if (experiment == null) throw new ArgumentNullException("experiment");
            if (string.IsNullOrEmpty(bodyName)) throw new ArgumentException("Must provide body", "bodyName");
            if (string.IsNullOrEmpty(subjectId)) throw new ArgumentException("Must provide subjectId", "subjectId");

            CelestialBodyScienceParams bodyParams;

            if (!CelestialBodyScienceParams.TryGetValue(bodyName, out bodyParams))
                throw new ArgumentException(bodyName + " not found", "bodyName");

            var subject = Substitute.For<IScienceSubject>();

            float subjectValue = GetSubjectValue(bodyParams, situation);

            subject.ScienceCap.Returns(experiment.scienceCap * subjectValue);
            subject.Science.Returns(alreadyCollectedScience);
            subject.DataScale.Returns(experiment.dataScale);
            subject.SubjectValue.Returns(subjectValue);
            subject.ScientificValue.Returns(1f);
            subject.Id.Returns(subjectId);

            float scientificValue = KspResearchAndDevelopmentDouble.GetSubjectValue(alreadyCollectedScience, subject);

            subject.ScientificValue.Returns(scientificValue);

            return subject;
        }


        private static float GetSubjectValue(CelestialBodyScienceParams scienceParams,
            ExperimentSituations situation)
        {
            switch (situation)
            {
                case ExperimentSituations.FlyingHigh:
                    return scienceParams.FlyingHighDataValue;
                case ExperimentSituations.FlyingLow:
                    return scienceParams.FlyingLowDataValue;
                case ExperimentSituations.InSpaceHigh:
                    return scienceParams.InSpaceHighDataValue;
                case ExperimentSituations.InSpaceLow:
                    return scienceParams.InSpaceLowDataValue;
                case ExperimentSituations.SrfLanded:
                    return scienceParams.LandedDataValue;
                case ExperimentSituations.SrfSplashed:
                    return scienceParams.SplashedDataValue;
                default:
                    throw new ArgumentException("Unknown experiment subjectSituation", "subjectSituation");

            }
        }
    }
}
