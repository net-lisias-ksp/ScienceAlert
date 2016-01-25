//using System;
//using System.Collections.ObjectModel;
//using System.Linq;
//using NSubstitute;
//using Ploeh.AutoFixture.Xunit;
//using ScienceAlert.Game;
//using ScienceAlert.VesselContext.Experiments.Sensors.Queries;
//using ScienceAlertTests.Game;
//using UnityEngine;
//using Xunit;
//using Xunit.Extensions;

//namespace ScienceAlertTests.VesselContext.Experiments.Sensors.Queries
//{
//    public class ScienceTransmissionValueSensorTests
//    {
//        [Fact()]
//        public void ScienceTransmissionValueSensorTest()
//        {
//            throw new NotImplementedException();
//        }

//        [Theory]
//        [InlineAutoData("mysteryGoo", 1f, "Kerbin", ExperimentSituations.SrfLanded, 0f, 0, 0.9000001f)]                         // 1 goo report on LP, no prev data, no prev reports, no modifiers
//        [InlineAutoData("mysteryGoo", 1f, "Kerbin", ExperimentSituations.SrfLanded, 0.9000001f, 0, (0.9810001f - 0.9000001f))]  // 1 goo report on LP, 1 prev xmit, no onboard, no modifiers
//        [InlineAutoData("mysteryGoo", 1f, "Kerbin", ExperimentSituations.SrfLanded, 0.9810001f, 0, 0f)]                         // 1 goo report on LP, 2 prev xmit, no onboard, no modifiers
//        [InlineAutoData("mysteryGoo", 1f, "Kerbin", ExperimentSituations.SrfLanded, 3f, 0, 0f)]                                 // 1 goo report on LP, 1 prev col, no onboard, no modifiers
//        public void ScienceTransmissionValueSensor_Get(
//            string experimentId, 
//            float careerMultiplier, 
//            string bodyName,
//            ExperimentSituations situation, 
//            float knownScience, 
//            int onboardREports, 
//            float expectedValue,
//            [Frozen] string subjectId)
//        {
                
//        }

//        //[Theory]
//        //[InlineAutoData("mysteryGoo", 1f, "Kerbin", ExperimentSituations.SrfLanded, 0f, 0, 0.9000001f)]                         // 1 goo report on LP, no prev data, no prev reports, no modifiers
//        //[InlineAutoData("mysteryGoo", 1f, "Kerbin", ExperimentSituations.SrfLanded, 0.9000001f, 0, (0.9810001f - 0.9000001f))]  // 1 goo report on LP, 1 prev xmit, no onboard, no modifiers
//        //[InlineAutoData("mysteryGoo", 1f, "Kerbin", ExperimentSituations.SrfLanded, 0.9810001f, 0, 0f)]                         // 1 goo report on LP, 2 prev xmit, no onboard, no modifiers
//        //[InlineAutoData("mysteryGoo", 1f, "Kerbin", ExperimentSituations.SrfLanded, 3f, 0, 0f)]                                 // 1 goo report on LP, 1 prev col, no onboard, no modifiers
//        //public void ScienceTransmissionValue_Get(
//        //    string experimentId,
//        //    float careerMultiplier,
//        //    string bodyName,
//        //    ExperimentSituations situation,
//        //    float knownScience,
//        //    int onboardReports,
//        //    float expectedTransmissionValue,
//        //    [Frozen] string subjectId)
//        //{
//        //    var researchAndDevDouble = new KspResearchAndDevelopmentDouble();
//        //    var experiment = ScienceCollectionValueSensorTests.ScienceExperimentFactory.Create(experimentId);
//        //    var subject = ScienceCollectionValueSensorTests.ScienceSubjectFactory.Create(experiment, bodyName, situation, subjectId, knownScience);

//        //    var subjectQuery = Substitute.For<IQueryScienceSubject>();
//        //    var scienceContainer = Substitute.For<IScienceDataContainer>();
//        //    var scienceData =
//        //        Enumerable.Repeat(new ScienceData(0f, 0f, 0f, subjectId, string.Empty), onboardReports).ToArray();

//        //    var scienceCollectionProvider = Substitute.For<IScienceContainerCollectionProvider>();

//        //    scienceContainer.GetData()
//        //        .Returns(scienceData);

//        //    scienceContainer.GetScienceCount().Returns(scienceData.Length);

//        //    subjectQuery.GetSubject(Arg.Is(experiment)).Returns(subject);
//        //    scienceCollectionProvider.Containers
//        //        .Returns(new ReadOnlyCollection<IScienceDataContainer>(new[] { scienceContainer }.ToList()));

//        //    var sut = new ScienceTransmissionValueSensor(experiment, subjectQuery, researchAndDevDouble, careerMultiplier, scienceCollectionProvider);

//        //    var result = sut.Passes();

//        //    Assert.True(Mathf.Approximately(expectedTransmissionValue, result), "Expected " + expectedTransmissionValue + ", got " + result);
//        //}
//    }
//}
