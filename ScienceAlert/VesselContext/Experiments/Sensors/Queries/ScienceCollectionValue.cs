using System;
using ScienceAlert.Core;
using ScienceAlert.Game;
using UnityEngine;

namespace ScienceAlert.VesselContext.Experiments.Sensors.Queries
{
    public class ScienceCollectionValue : IQuerySensorValue<float>
    {
        private readonly ScienceExperiment _experiment;
        private readonly IQueryScienceSubject _subject;
        private readonly IQueryScienceValue _queryScienceValue;
        private readonly float _careerScienceGainMultiplier;
        private readonly IQueryScienceDataForScienceSubject _onboardData;

        public ScienceCollectionValue(
            ScienceExperiment experiment,
            IQueryScienceSubject subject,
            IQueryScienceValue queryScienceValue,
            [Name(CoreKeys.CareerScienceGainMultiplier)] float careerScienceGainMultiplier,
            IQueryScienceDataForScienceSubject onboardData)
        {
            if (experiment == null) throw new ArgumentNullException("experiment");
            if (subject == null) throw new ArgumentNullException("subject");
            if (queryScienceValue == null) throw new ArgumentNullException("queryScienceValue");
            if (onboardData == null) throw new ArgumentNullException("onboardData");

            _experiment = experiment;
            _subject = subject;
            _queryScienceValue = queryScienceValue;
            _careerScienceGainMultiplier = careerScienceGainMultiplier;
            _onboardData = onboardData;
        }


        public float Get()
        {
            var subject = _subject.GetSubject(_experiment);

            return CalculateNextReportValue(subject, _onboardData.GetScienceData(subject).Count, GetTransmissionMultiplier());
        }


        protected virtual float GetTransmissionMultiplier()
        {
            return 1f;
        }


        private float CalculateNextReportValue(
            ScienceSubject subject,
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
    }
}
