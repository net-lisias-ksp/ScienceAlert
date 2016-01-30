using System;
using NSubstitute;
using Ploeh.AutoFixture.Xunit;
using ScienceAlert.Game;
using ScienceAlert.VesselContext.Experiments;
using Xunit;
using Xunit.Extensions;

namespace ScienceAlertTests.VesselContext.Experiments
{
    public class ExperimentSensorTests
    {
        [Theory, AutoDomainData]
        public void ExperimentSensor_Constructor_NullArg_Test(
            ScienceExperiment experiment, 
            IScienceSubjectProvider subjectProvider, 
            IExperimentReportValueCalculator reportValueCalculator)
        {
            Assert.Throws<ArgumentNullException>(
                () => new ExperimentSensor(null, subjectProvider, reportValueCalculator));
            Assert.Throws<ArgumentNullException>(() => new ExperimentSensor(experiment, null, reportValueCalculator));
            Assert.Throws<ArgumentNullException>(() => new ExperimentSensor(experiment, subjectProvider, null));
        }




        [Theory, AutoDomainData]
        public void UpdateSensorValues_UsesSubjectQueryOnlyOnce_Test([Frozen] IScienceSubjectProvider subjectProvider, ExperimentSensor sut) // to make sure we aren't being wasteful
        {
            sut.UpdateSensorValues();

            subjectProvider.Received(1).GetSubject(Arg.Any<ScienceExperiment>());
        }


        [Theory, AutoDomainData]
        public void UpdateSensorValues_DoesNotReportChanged_WhenAllCalculatedValuesSame(
            [Frozen] ScienceExperiment experiment,
            IScienceSubjectProvider subjectProvider, 
            IScienceSubject subject,
            IExperimentReportValueCalculator valueCalculator,
            float collectionValue,
            float transmissionValue,
            float labValue)
        {
            subjectProvider.GetSubject(Arg.Any<ScienceExperiment>()).Returns(subject);

            valueCalculator.CalculateCollectionValue(Arg.Is(experiment), Arg.Is(subject))
                .Returns(collectionValue);

            valueCalculator.CalculateTransmissionValue(Arg.Is(experiment), Arg.Is(subject))
                .Returns(transmissionValue);

            valueCalculator.CalculateLabValue(Arg.Is(experiment), Arg.Is(subject)).Returns(labValue);


            var sut = new ExperimentSensor(experiment, subjectProvider, valueCalculator);

            sut.ClearChangedFlag();
            sut.UpdateSensorValues(); // initially sets values

            Assert.Equal(collectionValue, sut.CollectionValue);
            Assert.Equal(transmissionValue, sut.TransmissionValue);
            Assert.Equal(labValue, sut.LabValue);

            sut.ClearChangedFlag();
            sut.UpdateSensorValues(); // shoudl result in no change
            var result = sut.HasChanged;

            Assert.False(result, "Incorrect status flag");


        }
    }
}
