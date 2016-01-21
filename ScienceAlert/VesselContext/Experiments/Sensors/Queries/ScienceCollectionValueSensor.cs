using System;
using System.Linq;
using ScienceAlert.Core;
using ScienceAlert.Game;
using UnityEngine;

namespace ScienceAlert.VesselContext.Experiments.Sensors.Queries
{
    public class ScienceCollectionValueSensor : IQuerySensorValue<float>
    {
        private readonly ScienceExperiment _experiment;
        private readonly IQueryScienceSubject _subject;
        private readonly IQueryScienceValue _queryScienceValue;
        private readonly float _careerScienceGainMultiplier;
        protected readonly IScienceContainerCollectionProvider _vesselContainer;

        public ScienceCollectionValueSensor(
            ScienceExperiment experiment,
            IQueryScienceSubject subject,
            IQueryScienceValue queryScienceValue,
            [Name(CoreKeys.CareerScienceGainMultiplier)] float careerScienceGainMultiplier,
            IScienceContainerCollectionProvider vesselContainer)
        {
            if (experiment == null) throw new ArgumentNullException("experiment");
            if (subject == null) throw new ArgumentNullException("subject");
            if (queryScienceValue == null) throw new ArgumentNullException("queryScienceValue");
            if (vesselContainer == null) throw new ArgumentNullException("vesselContainer");
            if (careerScienceGainMultiplier < 0f)
                throw new ArgumentOutOfRangeException("careerScienceGainMultiplier", careerScienceGainMultiplier,
                    "Career multiplier cannot be less than zero");

            _experiment = experiment;
            _subject = subject;
            _queryScienceValue = queryScienceValue;
            _careerScienceGainMultiplier = careerScienceGainMultiplier;
            _vesselContainer = vesselContainer;
        }


        public float Get()
        {
            var subject = _subject.GetSubject(_experiment);

            return CalculateNextReportValue(subject, GetNumberOnboardReports(subject.Id), GetTransmissionMultiplier());
        }


        protected virtual float GetTransmissionMultiplier()
        {
            return 1f;
        }


        private float CalculateNextReportValue(
            IScienceSubject subject,
            int onboardReportCount,
            float transmissionMultiplier)
        {
            if (subject == null) throw new ArgumentNullException("subject");

            var dataAmount = _experiment.baseValue * _experiment.dataScale;

            if (onboardReportCount == 0)
                return _queryScienceValue.GetScienceValue(dataAmount, subject, transmissionMultiplier) * _careerScienceGainMultiplier;

            var experimentValue =
                _queryScienceValue.GetNextScienceValue(dataAmount, subject, transmissionMultiplier) * _careerScienceGainMultiplier;

            if (onboardReportCount == 1)
                return experimentValue;

            return experimentValue / Mathf.Pow(4f, onboardReportCount - 1);
        }



        private static readonly ScienceData[] EmptyScienceArray = new ScienceData[0];

// ReSharper disable once InconsistentNaming
        private int GetNumberOnboardReports(string subjectID)
        {
            return _vesselContainer.Containers.SelectMany(
                container => container.GetScienceCount() > 0 ? container.GetData() : EmptyScienceArray)
                .Count(data => data.subjectID == subjectID);
        }
    }
}
