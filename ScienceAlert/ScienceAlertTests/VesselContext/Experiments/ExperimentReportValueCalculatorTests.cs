using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using NSubstitute;
using Ploeh.AutoFixture;
using Ploeh.AutoFixture.Xunit;
using ScienceAlert.Game;
using ScienceAlert.VesselContext.Experiments;
using ScienceAlertTests.Domain;
using ScienceAlertTests.Game;
using UnityEngine;
using Xunit;
using Xunit.Extensions;

namespace ScienceAlertTests.VesselContext.Experiments
{
    public class ExperimentReportValueCalculatorTests
    {
        static class Factory
        {
            public static ExperimentReportValueCalculator CreateSut(
                ReadOnlyCollection<ScienceExperiment> experiments, 
                float careerMultiplier, 
                IQueryScienceValue valueQuery,
                IVessel testVessel)
            {
                if (experiments == null) throw new ArgumentNullException("experiments");
                if (valueQuery == null) throw new ArgumentNullException("valueQuery");
                if (testVessel == null) throw new ArgumentNullException("testVessel");

                var homeworld = Substitute.For<ICelestialBody>();

                homeworld.BodyName.Returns("Kerbin");
                homeworld.Equals(Arg.Any<ICelestialBody>())
                    .Returns(ci => ci.Arg<ICelestialBody>().BodyName == homeworld.BodyName);

                var sut = new ExperimentReportValueCalculator(experiments, careerMultiplier, homeworld, valueQuery, testVessel);

                sut.OnActiveVesselModified();

                return sut;
            }
        }


        private static readonly string[] EmptyLabIds = new string[0];



        [Theory]
        [InlineAutoData("mysteryGoo", 1f, "Kerbin", ExperimentSituations.SrfLanded, 0f, 0, 3f)]                 // 1 goo report on launchpad, no previous data, no previous reports
        [InlineAutoData("mysteryGoo", .5f, "Kerbin", ExperimentSituations.SrfLanded, 0f, 0, 1.5f)]              // above, but science gain multiplier halved
        [InlineAutoData("mysteryGoo", 1f, "Kerbin", ExperimentSituations.SrfLanded, 0.9000001f, 0, 2.307693f)]  // 1 goo report, already transmitted 0.9 science points
        [InlineAutoData("mysteryGoo", 1f, "Kerbin", ExperimentSituations.SrfLanded, 0.9810001f, 0, 2.245384f)]  // 1 goo report, transmitted 0.981 points (scientist onboard)
        [InlineAutoData("mysteryGoo", 1f, "Kerbin", ExperimentSituations.SrfLanded, 0f, 2, 0.1730769f)]         // 1 goo report, no previous, 2 existing reports onboard
        [InlineAutoData("mysteryGoo", 1f, "Kerbin", ExperimentSituations.SrfLanded, 3f, 1, 0.1597631f)]         // 1 goo report, previous data of 3f, 1 existing report
        [InlineAutoData("mysteryGoo", 1f, "Kerbin", ExperimentSituations.InSpaceLow, 0f, 0, 10f)]               // 1 goo report from space
        [InlineAutoData("mysteryGoo", 1f, "Kerbin", ExperimentSituations.InSpaceLow, 0f, 1, 2.307693f)]         // 1 goo report from space with 1 existing report onboard
        [InlineAutoData("mysteryGoo", 1f, "Kerbin", ExperimentSituations.SrfSplashed, 0f, 0, 4f)]
        [InlineAutoData("mysteryGoo", 0.5f, "Kerbin", ExperimentSituations.SrfSplashed, 0f, 0, 2f)]
        [InlineAutoData("mysteryGoo", 1f, "Laythe", ExperimentSituations.SrfSplashed, 0f, 0, 120f)]
        [InlineAutoData("mysteryGoo", .25f, "Laythe", ExperimentSituations.SrfSplashed, 0f, 0, 30f)]
        public void CalculateCollectionValueTest(
            string experimentId,
            float careerMultiplier,
            string bodyName,
            ExperimentSituations experimentSituation,
            float knownScience,
            int onboardReportCount,
            float expectedResult,
            ScienceSubjectFactory subjectFactory,
            Fixture fixture)
        {
            var subjectId = fixture.Create(bodyName);
            var experiment = ScienceExperimentFactory.Create(experimentId);
            var subject = subjectFactory.Create(experiment, bodyName, experimentSituation, subjectId, knownScience);
            var scienceValue = new KspResearchAndDevelopmentDouble();

            var vessel = MockVessel(bodyName, experiment, experimentSituation, 5, 5, onboardReportCount, 0, subject, 0.5f, EmptyLabIds);

            var sut = Factory.CreateSut(new[] {experiment}.ToList().AsReadOnly(), careerMultiplier, scienceValue, vessel);

            var result = sut.CalculateCollectionValue(experiment, subject);

            Assert.True(Mathf.Approximately(expectedResult, result));
        }


        [Theory] 
        [InlineAutoData("mysteryGoo", 1f, 0.3f, "Kerbin", ExperimentSituations.SrfLanded, 0f, 0, 0.9000001f)]                         // 1 goo report on LP, no prev data, no prev reports, no modifiers
        [InlineAutoData("mysteryGoo", 1f, 0.3f, "Kerbin", ExperimentSituations.SrfLanded, 0.9000001f, 0, 0.08099991f)]  // 1 goo report on LP, 1 prev xmit, no onboard, no modifiers
        [InlineAutoData("mysteryGoo", 1f, 0.3f, "Kerbin", ExperimentSituations.SrfLanded, 0.9810001f, 0, 0f)]                         // 1 goo report on LP, 2 prev xmit, no onboard, no modifiers
        [InlineAutoData("mysteryGoo", 1f, 0.3f, "Kerbin", ExperimentSituations.SrfLanded, 3f, 0, 0f)]                                 // 1 goo report on LP, 1 prev col, no onboard, no modifiers
        [InlineAutoData("mysteryGoo", 1f, 0.3f, "Kerbin", ExperimentSituations.SrfSplashed, 0f, 0, 1.2f)]
        [InlineAutoData("mysteryGoo", 1f, 0.3f, "Laythe", ExperimentSituations.SrfSplashed, 0f, 0, 36f)]
        public void CalculateTransmissionValueTest(string experimentId,
            float careerMultiplier,
            float xmitMultiplier,
            string bodyName,
            ExperimentSituations experimentSituation,
            float knownScience,
            int onboardReportCount,
            float expectedResult,
            ScienceSubjectFactory subjectFactory,
            Fixture fixture)
        {
            var subjectId = fixture.Create(bodyName); 
            var experiment = ScienceExperimentFactory.Create(experimentId);
            var subject = subjectFactory.Create(experiment, bodyName, experimentSituation, subjectId, knownScience);
            var scienceValue = new KspResearchAndDevelopmentDouble();

            var vessel = MockVessel(bodyName, experiment, experimentSituation, 5, 5, onboardReportCount, 0, subject, xmitMultiplier, EmptyLabIds);

            var sut = Factory.CreateSut(new[] { experiment }.ToList().AsReadOnly(), careerMultiplier, scienceValue, vessel);

           
            var result = sut.CalculateTransmissionValue(experiment, subject);

            Assert.True(Mathf.Approximately(expectedResult, result), "Expected " + expectedResult + " but got " + result);
        }


        [Theory]
        [InlineAutoData("mobileMaterialsLab", 1f, "Kerbin", ExperimentSituations.SrfLanded, 1f)]
        [InlineAutoData("mobileMaterialsLab", 0.5f, "Kerbin", ExperimentSituations.SrfLanded, 1f)] 
        [InlineAutoData("mysteryGoo", 1f, "Kerbin", ExperimentSituations.InSpaceHigh, 19f)]
        [InlineAutoData("mysteryGoo", 0.5f, "Kerbin", ExperimentSituations.InSpaceHigh, 9f)]
        [InlineAutoData("mobileMaterialsLab", 1f, "Kerbin", ExperimentSituations.InSpaceHigh, 47f)]
        [InlineAutoData("mobileMaterialsLab", 0.5f, "Kerbin", ExperimentSituations.InSpaceHigh, 23f)]
        [InlineAutoData("mobileMaterialsLab", 1f, "Laythe", ExperimentSituations.SrfSplashed, 375f)]
        public void CalculateLabValue_OneLabWithNoData_Test(
            string experimentId,
            float careerMultiplier,
            string bodyName,
            ExperimentSituations situation,
            float expectedResult,
            ScienceSubjectFactory subjectFactory,
            Fixture fixture)
        {
            var subjectId = fixture.Create(bodyName);

            var experiment = ScienceExperimentFactory.Create(experimentId);
            var subject = subjectFactory.Create(experiment, bodyName, situation, subjectId, 0f);
            var scienceValue = new KspResearchAndDevelopmentDouble();

            var vessel = MockVessel(bodyName, experiment, situation, 5, 5, 0, 1, subject, 1f, EmptyLabIds);

            var sut = Factory.CreateSut(new[] { experiment }.ToList().AsReadOnly(), careerMultiplier, scienceValue, vessel);


            var result = sut.CalculateLabValue(experiment, subject);

            Assert.True(Mathf.Approximately(expectedResult, result), "Expected " + expectedResult + " but got " + result);
        }


      


        [Theory]
        [InlineAutoData("mobileMaterialsLab", 1f, "Kerbin", ExperimentSituations.SrfLanded)]
        [InlineAutoData("mobileMaterialsLab", 0.5f, "Kerbin", ExperimentSituations.SrfLanded)]
        [InlineAutoData("mysteryGoo", 1f, "Kerbin", ExperimentSituations.InSpaceHigh)]
        [InlineAutoData("mysteryGoo", 0.25f, "Kerbin", ExperimentSituations.InSpaceHigh)]
        [InlineAutoData("mysteryGoo", 1f, "Laythe", ExperimentSituations.SrfSplashed)]
        [InlineAutoData("mysteryGoo", 0.5f, "Laythe", ExperimentSituations.SrfSplashed)]
        public void CalculateLabValue_WithNoLabsAvailable_Test(
            string experimentId,
            float careerMultiplier,
            string bodyName,
            ExperimentSituations situation,
            ScienceSubjectFactory subjectFactory,
            Fixture fixture)
        {
            var subjectId = fixture.Create(bodyName);
            var experiment = ScienceExperimentFactory.Create(experimentId);
            var subject = subjectFactory.Create(experiment, bodyName, situation, subjectId, 0f);
            var scienceValue = new KspResearchAndDevelopmentDouble();

            var vessel = MockVessel(bodyName, experiment, situation, 5, 5, 0, 0, subject, 1f, EmptyLabIds);

            var sut = Factory.CreateSut(new[] { experiment }.ToList().AsReadOnly(), careerMultiplier, scienceValue, vessel);


            var result = sut.CalculateLabValue(experiment, subject);

            Assert.True(result < float.Epsilon, "Expected zero, got " + result);
        }


        [Theory]
        [InlineAutoData("mobileMaterialsLab", 1f, "Kerbin", ExperimentSituations.SrfLanded, 1f)]
        [InlineAutoData("mobileMaterialsLab", 0.5f, "Kerbin", ExperimentSituations.SrfLanded, 1f)]
        [InlineAutoData("mobileMaterialsLab", 1f, "Kerbin", ExperimentSituations.SrfSplashed, 1f)]
        [InlineAutoData("mysteryGoo", 1f, "Kerbin", ExperimentSituations.InSpaceHigh, 19f)]
        [InlineAutoData("mysteryGoo", 0.5f, "Kerbin", ExperimentSituations.InSpaceHigh, 9f)]
        [InlineAutoData("mysteryGoo", 1f, "Kerbin", ExperimentSituations.SrfSplashed, 0f)]
        [InlineAutoData("mysteryGoo", 1f, "Laythe", ExperimentSituations.SrfSplashed, 150f)]
        [InlineAutoData("mysteryGoo", 0.5f, "Laythe", ExperimentSituations.SrfSplashed, 75f)]
        public void CalculateLabValue_WithOneLab_ThatHasAnalyzedOtherData_Test( // this lab should be available for use
            string experimentId,
            float careerMultiplier,
            string bodyName,
            ExperimentSituations situation,
            float expectedResult,
            ScienceSubjectFactory subjectFactory,
            Fixture fixture)
        {
            var subjectId = fixture.Create(bodyName);
            var analyzedIds = fixture.CreateMany(bodyName);

            var experiment = ScienceExperimentFactory.Create(experimentId);
            var subject = subjectFactory.Create(experiment, bodyName, situation, subjectId, 0f);
            var scienceValue = new KspResearchAndDevelopmentDouble();

            var vessel = MockVessel(bodyName, experiment, situation, 5, 5, 0, 1, subject, 1f, analyzedIds);

            var sut = Factory.CreateSut(new[] { experiment }.ToList().AsReadOnly(), careerMultiplier, scienceValue, vessel);


            var result = sut.CalculateLabValue(experiment, subject);

            Assert.True(Mathf.Approximately(expectedResult, result), "Expected " + expectedResult + " but got " + result);
        }


        [Theory]
        [InlineAutoData("mobileMaterialsLab", 1f, "Kerbin", ExperimentSituations.SrfLanded, 0f)]
        [InlineAutoData("mobileMaterialsLab", 0.5f, "Kerbin", ExperimentSituations.SrfLanded, 0f)]
        [InlineAutoData("mysteryGoo", 1f, "Kerbin", ExperimentSituations.InSpaceHigh, 0f)]
        [InlineAutoData("mysteryGoo", 0.25f, "Kerbin", ExperimentSituations.InSpaceHigh, 0f)]
        [InlineAutoData("mysteryGoo", 1f, "Laythe", ExperimentSituations.SrfSplashed, 0f)]
        [InlineAutoData("mysteryGoo", 0.5f, "Laythe", ExperimentSituations.SrfSplashed, 0f)]
        public void CalculateLabValue_WithTwoLabs_ThatAlreadyAnalyzedData_Test(
            string experimentId,
            float careerMultiplier,
            string bodyName,
            ExperimentSituations situation,
            float expectedResult,
            ScienceSubjectFactory subjectFactory,
            Fixture fixture)
        {
            var subjectId = fixture.Create(bodyName);
            var analyzedIds = fixture.CreateMany(bodyName);

            var otherIds = analyzedIds.Union(new [] { subjectId }); 

            var experiment = ScienceExperimentFactory.Create(experimentId);
            var subject = subjectFactory.Create(experiment, bodyName, situation, subjectId, 0f);
            var scienceValue = new KspResearchAndDevelopmentDouble();

            var vessel = MockVessel(bodyName, experiment, situation, 5, 5, 0, 2, subject, 1f, otherIds);

            var sut = Factory.CreateSut(new[] { experiment }.ToList().AsReadOnly(), careerMultiplier, scienceValue, vessel);


            var result = sut.CalculateLabValue(experiment, subject);

            Assert.True(result < float.Epsilon, "Expected zero, got " + result);
        }


       

        private IVessel MockVessel(
            string bodyName,
            ScienceExperiment experiment, 
            ExperimentSituations situation,
            int numScienceExperimentModules, 
            int numScienceContainers, 
            int numStoredReports,
            int numLabs,
            IScienceSubject subject, float xmitScalar, IEnumerable<string> labDataIds)
        {
            var scienceModules = MockScienceModules(experiment.id, numScienceExperimentModules, xmitScalar).ToList();
            var dataContainers = MockScienceContainers(numScienceContainers, experiment, Enumerable.Repeat(subject, numStoredReports)).ToList();
            var labs = MockScienceLabs(numLabs, labDataIds).ToList();

            var orbitingBody = Substitute.For<ICelestialBody>();
            orbitingBody.BodyName.Returns(bodyName);
            orbitingBody.Equals(Arg.Any<ICelestialBody>()).Returns(ci => Arg.Any<ICelestialBody>().BodyName == bodyName);

            var vessel = Substitute.For<IVessel>();

            vessel.ScienceExperimentModules.Returns(new ReadOnlyCollection<IModuleScienceExperiment>(scienceModules));
            vessel.Containers.Returns(new ReadOnlyCollection<IScienceDataContainer>(dataContainers));
            vessel.Labs.Returns(new ReadOnlyCollection<IScienceLab>(labs));
            vessel.Landed.Returns(situation == ExperimentSituations.SrfLanded);
            vessel.SplashedDown.Returns(situation == ExperimentSituations.SrfSplashed);
            vessel.OrbitingBody.Returns(orbitingBody);

            return vessel;
        }


        private IEnumerable<IModuleScienceExperiment> MockScienceModules(string experimentId, int count, float xmitScalar)
        {
            var modules = new List<IModuleScienceExperiment>();

            for (int i = 0; i < count; ++i)
            {
                var module = Substitute.For<IModuleScienceExperiment>();

                module.ExperimentID.Returns(experimentId);
                module.TransmissionMultiplier.Returns(xmitScalar);
                module.Deployed.Returns(false);
                module.CanBeDeployed.Returns(true);

                modules.Add(module);
            }

            return modules;
        }


        private IEnumerable<IScienceDataContainer> MockScienceContainers(int containerCount, ScienceExperiment experiment, IEnumerable<IScienceSubject> storedDataSubjects)
        {
            var containers = new List<IScienceDataContainer>();

            for (int i = 0; i < containerCount; ++i)
            {
                var container = Substitute.For<IScienceDataContainer>();

                container.GetScienceCount().Returns(0);
                container.GetData().Returns(Enumerable.Empty<ScienceData>().ToArray());

                containers.Add(container);
            }

            var storedSubjects =
                storedDataSubjects.Select(
                    subject =>
                        new ScienceData(new ConfigNode()) {dataAmount = experiment.baseValue * experiment.dataScale, subjectID = subject.Id})
                    .ToArray();

            if (storedSubjects.Any() && containerCount <= 0)
                throw new ArgumentException("No containers to store data subjects in");

            containers.First().GetScienceCount().Returns(storedSubjects.Length);
            containers.First().GetData().Returns(storedSubjects);

            return containers;
        }



        private IEnumerable<IScienceLab> MockScienceLabs(int count, IEnumerable<string> analyzedData)
        {
            var labs = new List<IScienceLab>();
            var data = analyzedData.ToList();

            for (int i = 0; i < count; ++i)
            {
                var lab = Substitute.For<IScienceLab>();

                lab.HomeworldMultiplier.Returns(0.1f);
                lab.ContextBonus.Returns(0.25f);
                lab.SurfaceBonus.Returns(0.1f);

                lab.HasAnalyzedSubject(Arg.Any<IScienceSubject>())
                    .Returns(ci => data.Any(dataId => dataId == ci.Arg<IScienceSubject>().Id));

                labs.Add(lab);
            }

            return labs;
        }
    }
}
