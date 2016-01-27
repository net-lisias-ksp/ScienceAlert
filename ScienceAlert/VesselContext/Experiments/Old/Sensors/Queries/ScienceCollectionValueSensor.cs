//using System;
//using System.Linq;
//using ScienceAlert.Core;
//using ScienceAlert.Game;
//using UnityEngine;

//namespace ScienceAlert.VesselContext.Experiments.Sensors.Queries
//{
//    public class ScienceCollectionValueSensor : IQuerySensorValue<float>
//    {
//        protected readonly ScienceExperiment Experiment;
//        private readonly IScienceSubjectProvider _subject;
//        private readonly IQueryScienceValue _queryScienceValue;
//        private readonly float _careerScienceGainMultiplier;
//        private readonly IScienceContainerCollectionProvider _vesselContainer;

//        public ScienceCollectionValueSensor(
//            ScienceExperiment experiment,
//            IScienceSubjectProvider subject,
//            IQueryScienceValue queryScienceValue,
//            [Name(CoreKeys.CareerScienceGainMultiplier)] float careerScienceGainMultiplier,
//            IScienceContainerCollectionProvider vesselContainer)
//        {
//            if (experiment == null) throw new ArgumentNullException("experiment");
//            if (subject == null) throw new ArgumentNullException("subject");
//            if (queryScienceValue == null) throw new ArgumentNullException("queryScienceValue");
//            if (vesselContainer == null) throw new ArgumentNullException("vesselContainer");
//            if (careerScienceGainMultiplier < 0f)
//                throw new ArgumentOutOfRangeException("careerScienceGainMultiplier", careerScienceGainMultiplier,
//                    "Career multiplier cannot be less than zero");

//            Experiment = experiment;
//            _subject = subject;
//            _queryScienceValue = queryScienceValue;
//            _careerScienceGainMultiplier = careerScienceGainMultiplier;
//            _vesselContainer = vesselContainer;
//        }


//        public float Passes()
//        {
//            var subject = _subject.GetSubject(Experiment);

//            return CalculateNextReportValue(subject, GetNumberOnboardReports(subject.Id), GetTransmissionMultiplier());
//        }


//        protected virtual float GetTransmissionMultiplier()
//        {
//            return 1f;
//        }


//        private float CalculateNextReportValue(
//            IScienceSubject subject,
//            int onboardReportCount,
//            float transmissionMultiplier)
//        {
//            if (subject == null) throw new ArgumentNullException("subject");

//            var dataAmount = Experiment.baseValue * Experiment.dataScale;

//            if (onboardReportCount == 0)
//                return _queryScienceValue.GetScienceValue(dataAmount, subject, transmissionMultiplier) * _careerScienceGainMultiplier;

//            var experimentValue =
//                _queryScienceValue.GetNextScienceValue(dataAmount, subject, transmissionMultiplier) * _careerScienceGainMultiplier;

//            if (onboardReportCount == 1)
//                return experimentValue;

//            return experimentValue / Mathf.Pow(4f, onboardReportCount - 1);
//        }



//        private static readonly ScienceData[] EmptyScienceArray = new ScienceData[0];

//// ReSharper disable once InconsistentNaming
//        private int GetNumberOnboardReports(string subjectID)
//        {
//            return _vesselContainer.Containers.SelectMany(
//                container => container.GetScienceCount() > 0 ? container.GetData() : EmptyScienceArray)
//                .Count(data => data.subjectID == subjectID);
//        }
//    }
//}
