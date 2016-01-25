//using System;
//using System.Collections.Generic;
//using System.Collections.ObjectModel;
//using System.Linq;
//using FinePrint;
//using KSPAchievements;
//using NSubstitute;
//using Ploeh.AutoFixture;
//using Ploeh.AutoFixture.Xunit;
//using ScienceAlert.Game;
//using ScienceAlert.VesselContext.Experiments.Sensors.Queries;
//using ScienceAlertTests.Game;
//using UnityEngine;
//using Xunit;
//using Xunit.Extensions;
//using Object = System.Object;

//namespace ScienceAlertTests.VesselContext.Experiments.Sensors.Queries
//{
//    public class ScienceCollectionValueSensorTests
//    {
//        [Theory, AutoDomainData]
//        public void ScienceCollectionValueTest(ScienceExperiment experiment, IQueryScienceSubject subjectQuery, IQueryScienceValue valueQuery, float careerMultiplier, IScienceContainerCollectionProvider scienceContainerProvider)
//        {
//            Assert.Throws<ArgumentNullException>(
//                () => new ScienceCollectionValueSensor(null, subjectQuery, valueQuery, careerMultiplier, scienceContainerProvider));

//            Assert.Throws<ArgumentNullException>(
//                () => new ScienceCollectionValueSensor(experiment, null, valueQuery, careerMultiplier, scienceContainerProvider));

//            Assert.Throws<ArgumentNullException>(
//                () => new ScienceCollectionValueSensor(experiment, subjectQuery, null, careerMultiplier, scienceContainerProvider));

//            Assert.Throws<ArgumentNullException>(
//                () => new ScienceCollectionValueSensor(experiment, subjectQuery, valueQuery, careerMultiplier, null));

//            Assert.Throws<ArgumentOutOfRangeException>(
//                () => new ScienceCollectionValueSensor(experiment, subjectQuery, valueQuery, -1f, scienceContainerProvider));
//        }


//        public static ScienceCollectionValueSensor CreateSut(string experimentId, string bodyName, ExperimentSituations subjectSituation, string subjectId, float careerMultiplier, float knownScience, int onboardReports)
//        {
//            var researchAndDevDouble = new KspResearchAndDevelopmentDouble();
//            var experiment = ScienceExperimentFactory.Create(experimentId);
//            var subject = ScienceSubjectFactory.Create(experiment, bodyName, subjectSituation, subjectId, knownScience);

//            var subjectQuery = Substitute.For<IQueryScienceSubject>();
//            var scienceContainer = Substitute.For<IScienceDataContainer>();
//            var scienceData =
//                Enumerable.Repeat(new ScienceData(0f, 0f, 0f, subjectId, string.Empty), onboardReports).ToArray();

//            var scienceCollectionProvider = Substitute.For<IScienceContainerCollectionProvider>();

//            scienceContainer.GetData()
//                .Returns(scienceData);

//            scienceContainer.GetScienceCount().Returns(scienceData.Length);

//            subjectQuery.GetSubject(Arg.Is(experiment)).Returns(subject);
//            scienceCollectionProvider.Containers
//                .Returns(new ReadOnlyCollection<IScienceDataContainer>(new[] { scienceContainer }.ToList()));

//            return new ScienceCollectionValueSensor(experiment, subjectQuery, researchAndDevDouble, careerMultiplier, scienceCollectionProvider);
//        }


//        [Theory]
//        [InlineAutoData("mysteryGoo", 1f, "Kerbin", ExperimentSituations.SrfLanded, 0f, 0, 3f)]                 // 1 goo report on launchpad, no previous data, no previous reports
//        [InlineAutoData("mysteryGoo", .5f, "Kerbin", ExperimentSituations.SrfLanded, 0f, 0, 1.5f)]              // above, but science gain multiplier halved
//        [InlineAutoData("mysteryGoo", 1f, "Kerbin", ExperimentSituations.SrfLanded, 0.9000001f, 0, 2.307693f)]  // 1 goo report, already transmitted 0.9 science points
//        [InlineAutoData("mysteryGoo", 1f, "Kerbin", ExperimentSituations.SrfLanded, 0.9810001f, 0, 2.245384f)]  // 1 goo report, transmitted 0.981 points (scientist onboard)
//        [InlineAutoData("mysteryGoo", 1f, "Kerbin", ExperimentSituations.SrfLanded, 0f, 2, 0.1730769f)]         // 1 goo report, no previous, 2 existing reports onboard
//        [InlineAutoData("mysteryGoo", 1f, "Kerbin", ExperimentSituations.SrfLanded, 3f, 1, 0.1597633f)]         // 1 goo report, previous data of 3f, 1 existing report
//        [InlineAutoData("mysteryGoo", 1f, "Kerbin", ExperimentSituations.InSpaceLow, 0f, 0, 10f)]               // 1 goo report from space
//        [InlineAutoData("mysteryGoo", 1f, "Kerbin", ExperimentSituations.InSpaceLow, 0f, 1, 2.307693f)]         // 1 goo report from space with 1 existing report onboard
//        public void ScienceCollectionValue_Get(
//            string experimentId,
//            float careerMultiplier, 
//            string bodyName,
//            ExperimentSituations situation,
//            float knownScience,
//            int onboardReports, 
//            float expectedCollectionValue,
//            [Frozen] string subjectId)
//        {
//            //var researchAndDevDouble = new KspResearchAndDevelopmentDouble();
//            //var experiment = ScienceExperimentFactory.Create(experimentId);
//            //var subject = ScienceSubjectFactory.Create(experiment, bodyName, situation, subjectId, knownScience);

//            //var subjectQuery = Substitute.For<IQueryScienceSubject>();
//            //var scienceContainer = Substitute.For<IScienceDataContainer>();
//            //var scienceData =
//            //    Enumerable.Repeat(new ScienceData(0f, 0f, 0f, subjectId, string.Empty), onboardReports).ToArray();

//            //var scienceCollectionProvider = Substitute.For<IScienceContainerCollectionProvider>();

//            //scienceContainer.GetData()
//            //    .Returns(scienceData);

//            //scienceContainer.GetScienceCount().Returns(scienceData.Length);

//            //subjectQuery.GetSubject(Arg.Is(experiment)).Returns(subject);
//            //scienceCollectionProvider.Containers
//            //    .Returns(new ReadOnlyCollection<IScienceDataContainer>(new[] {scienceContainer}.ToList()));

//            //var sut = new ScienceCollectionValueSensor(experiment, subjectQuery, researchAndDevDouble, careerMultiplier, scienceCollectionProvider);

//            var sut = CreateSut(experimentId, bodyName, situation, subjectId, careerMultiplier, knownScience,
//                onboardReports);

//            var result = sut.Passes();

//            Assert.True(Mathf.Approximately(expectedCollectionValue, result), "Expected " + expectedCollectionValue + ", got " + result);
//        }


//        /*
//         * [LOG 07:11:27.498] ------------------------------------------
//[LOG 07:11:27.510] Body: Sun
//[LOG 07:11:27.522] ------------------------------------------
//[LOG 07:11:27.535] LandedDataValue: 1
//[LOG 07:11:27.547] SplashedDataValue: 1
//[LOG 07:11:27.560] FlyingLowDataValue: 1
//[LOG 07:11:27.573] FlyingHighDataValue: 1
//[LOG 07:11:27.586] InSpaceLowDataValue: 11
//[LOG 07:11:27.599] InSpaceHighDataValue: 2
//[LOG 07:11:27.612] RecoveryValue: 4
//[LOG 07:11:27.624] flyingAltitudeThreshold: 18000
//[LOG 07:11:27.637] spaceAltitudeThreshold: 1E+09
//[LOG 07:11:27.650] ------------------------------------------
//[LOG 07:11:27.663] Body: Kerbin
//[LOG 07:11:27.675] ------------------------------------------

//[LOG 07:11:27.802] ------------------------------------------
//[LOG 07:11:27.815] Body: Mun
//[LOG 07:11:27.827] ------------------------------------------
//[LOG 07:11:27.840] LandedDataValue: 4
//[LOG 07:11:27.853] SplashedDataValue: 1
//[LOG 07:11:27.866] FlyingLowDataValue: 1
//[LOG 07:11:27.879] FlyingHighDataValue: 1
//[LOG 07:11:27.891] InSpaceLowDataValue: 3
//[LOG 07:11:27.905] InSpaceHighDataValue: 2
//[LOG 07:11:27.917] RecoveryValue: 2
//[LOG 07:11:27.931] flyingAltitudeThreshold: 18000
//[LOG 07:11:27.945] spaceAltitudeThreshold: 60000
//[LOG 07:11:27.957] ------------------------------------------
//[LOG 07:11:27.970] Body: Minmus
//[LOG 07:11:27.982] ------------------------------------------
//[LOG 07:11:27.994] LandedDataValue: 5
//[LOG 07:11:28.007] SplashedDataValue: 1
//[LOG 07:11:28.019] FlyingLowDataValue: 1
//[LOG 07:11:28.032] FlyingHighDataValue: 1
//[LOG 07:11:28.049] InSpaceLowDataValue: 4
//[LOG 07:11:28.062] InSpaceHighDataValue: 2.5
//[LOG 07:11:28.075] RecoveryValue: 2.5
//[LOG 07:11:28.087] flyingAltitudeThreshold: 18000
//[LOG 07:11:28.101] spaceAltitudeThreshold: 30000
//[LOG 07:11:28.113] ------------------------------------------
//[LOG 07:11:28.129] Body: Moho
//[LOG 07:11:28.142] ------------------------------------------
//[LOG 07:11:28.155] LandedDataValue: 10
//[LOG 07:11:28.168] SplashedDataValue: 1
//[LOG 07:11:28.181] FlyingLowDataValue: 1
//[LOG 07:11:28.195] FlyingHighDataValue: 1
//[LOG 07:11:28.208] InSpaceLowDataValue: 8
//[LOG 07:11:28.221] InSpaceHighDataValue: 7
//[LOG 07:11:28.234] RecoveryValue: 7
//[LOG 07:11:28.247] flyingAltitudeThreshold: 18000
//[LOG 07:11:28.260] spaceAltitudeThreshold: 80000
//[LOG 07:11:28.273] ------------------------------------------
//[LOG 07:11:28.285] Body: Eve
//[LOG 07:11:28.297] ------------------------------------------
//[LOG 07:11:28.309] LandedDataValue: 8
//[LOG 07:11:28.323] SplashedDataValue: 8
//[LOG 07:11:28.337] FlyingLowDataValue: 6
//[LOG 07:11:28.350] FlyingHighDataValue: 6
//[LOG 07:11:28.363] InSpaceLowDataValue: 7
//[LOG 07:11:28.375] InSpaceHighDataValue: 5
//[LOG 07:11:28.388] RecoveryValue: 5
//[LOG 07:11:28.400] flyingAltitudeThreshold: 22000
//[LOG 07:11:28.413] spaceAltitudeThreshold: 400000
//[LOG 07:11:28.425] ------------------------------------------
//[LOG 07:11:28.437] Body: Duna
//[LOG 07:11:28.449] ------------------------------------------

//[LOG 07:11:28.577] ------------------------------------------
//[LOG 07:11:28.588] Body: Ike
//[LOG 07:11:28.600] ------------------------------------------
//[LOG 07:11:28.613] LandedDataValue: 8
//[LOG 07:11:28.626] SplashedDataValue: 1
//[LOG 07:11:28.638] FlyingLowDataValue: 1
//[LOG 07:11:28.651] FlyingHighDataValue: 1
//[LOG 07:11:28.664] InSpaceLowDataValue: 7
//[LOG 07:11:28.676] InSpaceHighDataValue: 5
//[LOG 07:11:28.689] RecoveryValue: 5
//[LOG 07:11:28.702] flyingAltitudeThreshold: 18000
//[LOG 07:11:28.715] spaceAltitudeThreshold: 50000
//[LOG 07:11:28.727] ------------------------------------------
//[LOG 07:11:28.741] Body: Jool
//[LOG 07:11:28.753] ------------------------------------------
//[LOG 07:11:28.765] LandedDataValue: 30
//[LOG 07:11:28.779] SplashedDataValue: 1
//[LOG 07:11:28.792] FlyingLowDataValue: 12
//[LOG 07:11:28.805] FlyingHighDataValue: 9
//[LOG 07:11:28.819] InSpaceLowDataValue: 7
//[LOG 07:11:28.832] InSpaceHighDataValue: 6
//[LOG 07:11:28.846] RecoveryValue: 6
//[LOG 07:11:28.859] flyingAltitudeThreshold: 120000
//[LOG 07:11:28.872] spaceAltitudeThreshold: 4000000
//[LOG 07:11:28.885] ------------------------------------------
//[LOG 07:11:28.898] Body: Laythe
//[LOG 07:11:28.910] ------------------------------------------
//[LOG 07:11:28.922] LandedDataValue: 14
//[LOG 07:11:28.934] SplashedDataValue: 12
//[LOG 07:11:28.947] FlyingLowDataValue: 11
//[LOG 07:11:28.959] FlyingHighDataValue: 10
//[LOG 07:11:28.972] InSpaceLowDataValue: 9
//[LOG 07:11:28.985] InSpaceHighDataValue: 8
//[LOG 07:11:28.998] RecoveryValue: 8
//[LOG 07:11:29.011] flyingAltitudeThreshold: 10000
//[LOG 07:11:29.024] spaceAltitudeThreshold: 200000
//[LOG 07:11:29.036] ------------------------------------------
//[LOG 07:11:29.049] Body: Vall
//[LOG 07:11:29.061] ------------------------------------------
//[LOG 07:11:29.073] LandedDataValue: 12
//[LOG 07:11:29.086] SplashedDataValue: 1
//[LOG 07:11:29.099] FlyingLowDataValue: 1
//[LOG 07:11:29.112] FlyingHighDataValue: 1
//[LOG 07:11:29.125] InSpaceLowDataValue: 9
//[LOG 07:11:29.137] InSpaceHighDataValue: 8
//[LOG 07:11:29.150] RecoveryValue: 8
//[LOG 07:11:29.163] flyingAltitudeThreshold: 18000
//[LOG 07:11:29.176] spaceAltitudeThreshold: 90000
//[LOG 07:11:29.189] ------------------------------------------
//[LOG 07:11:29.201] Body: Bop
//[LOG 07:11:29.213] ------------------------------------------
//[LOG 07:11:29.226] LandedDataValue: 12
//[LOG 07:11:29.238] SplashedDataValue: 1
//[LOG 07:11:29.251] FlyingLowDataValue: 1
//[LOG 07:11:29.264] FlyingHighDataValue: 1
//[LOG 07:11:29.276] InSpaceLowDataValue: 9
//[LOG 07:11:29.289] InSpaceHighDataValue: 8
//[LOG 07:11:29.302] RecoveryValue: 8
//[LOG 07:11:29.315] flyingAltitudeThreshold: 18000
//[LOG 07:11:29.327] spaceAltitudeThreshold: 25000
//[LOG 07:11:29.340] ------------------------------------------
//[LOG 07:11:29.352] Body: Tylo
//[LOG 07:11:29.364] ------------------------------------------
//[LOG 07:11:29.376] LandedDataValue: 12
//[LOG 07:11:29.389] SplashedDataValue: 1
//[LOG 07:11:29.401] FlyingLowDataValue: 1
//[LOG 07:11:29.414] FlyingHighDataValue: 1
//[LOG 07:11:29.426] InSpaceLowDataValue: 10
//[LOG 07:11:29.439] InSpaceHighDataValue: 8
//[LOG 07:11:29.451] RecoveryValue: 8
//[LOG 07:11:29.464] flyingAltitudeThreshold: 18000
//[LOG 07:11:29.477] spaceAltitudeThreshold: 250000
//[LOG 07:11:29.489] ------------------------------------------
//[LOG 07:11:29.502] Body: Gilly
//[LOG 07:11:29.514] ------------------------------------------
//[LOG 07:11:29.528] LandedDataValue: 9
//[LOG 07:11:29.540] SplashedDataValue: 1
//[LOG 07:11:29.553] FlyingLowDataValue: 1
//[LOG 07:11:29.566] FlyingHighDataValue: 1
//[LOG 07:11:29.579] InSpaceLowDataValue: 8
//[LOG 07:11:29.592] InSpaceHighDataValue: 6
//[LOG 07:11:29.607] RecoveryValue: 6
//[LOG 07:11:29.624] flyingAltitudeThreshold: 18000
//[LOG 07:11:29.637] spaceAltitudeThreshold: 6000
//[LOG 07:11:29.655] ------------------------------------------
//[LOG 07:11:29.668] Body: Pol
//[LOG 07:11:29.679] ------------------------------------------
//[LOG 07:11:29.692] LandedDataValue: 12
//[LOG 07:11:29.704] SplashedDataValue: 1
//[LOG 07:11:29.719] FlyingLowDataValue: 1
//[LOG 07:11:29.732] FlyingHighDataValue: 1
//[LOG 07:11:29.747] InSpaceLowDataValue: 9
//[LOG 07:11:29.761] InSpaceHighDataValue: 8
//[LOG 07:11:29.775] RecoveryValue: 8
//[LOG 07:11:29.788] flyingAltitudeThreshold: 18000
//[LOG 07:11:29.801] spaceAltitudeThreshold: 22000
//[LOG 07:11:29.814] ------------------------------------------
//[LOG 07:11:29.826] Body: Dres
//[LOG 07:11:29.842] ------------------------------------------
//[LOG 07:11:29.855] LandedDataValue: 8
//[LOG 07:11:29.867] SplashedDataValue: 1
//[LOG 07:11:29.880] FlyingLowDataValue: 1
//[LOG 07:11:29.893] FlyingHighDataValue: 1
//[LOG 07:11:29.905] InSpaceLowDataValue: 7
//[LOG 07:11:29.918] InSpaceHighDataValue: 6
//[LOG 07:11:29.931] RecoveryValue: 6
//[LOG 07:11:29.944] flyingAltitudeThreshold: 18000
//[LOG 07:11:29.957] spaceAltitudeThreshold: 25000
//[LOG 07:11:29.970] ------------------------------------------
//[LOG 07:11:29.982] Body: Eeloo
//[LOG 07:11:29.994] ------------------------------------------
//[LOG 07:11:30.007] LandedDataValue: 15
//[LOG 07:11:30.020] SplashedDataValue: 1
//[LOG 07:11:30.033] FlyingLowDataValue: 1
//[LOG 07:11:30.045] FlyingHighDataValue: 1
//[LOG 07:11:30.058] InSpaceLowDataValue: 12
//[LOG 07:11:30.070] InSpaceHighDataValue: 10
//[LOG 07:11:30.083] RecoveryValue: 10
//[LOG 07:11:30.095] flyingAltitudeThreshold: 18000
//[LOG 07:11:30.108] spaceAltitudeThreshold: 60000
//         * */
//        


//    }
//}
