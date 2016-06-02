using System;
using NSubstitute;
using Ploeh.AutoFixture.Xunit;
using ScienceAlert.Game;
using ScienceAlert.VesselContext.Experiments;
using Xunit.Extensions;

namespace ScienceAlertTests.VesselContext.Experiments
{
    public class ExperimentSensorTests
    {
        [Theory, AutoDomainData]
        public void ExperimentSensor_Constructor_NullArg_Test(
            ScienceExperiment experiment, 
            IExistingScienceSubjectProvider subjectProvider, 
            IExperimentReportValueCalculator reportValueCalculator)
        {
            throw new NotImplementedException();

            //Assert.Throws<ArgumentNullException>(
            //    () => new ExperimentSensor(null, subjectProvider, reportValueCalculator));
            //Assert.Throws<ArgumentNullException>(() => new ExperimentSensor(experiment, null, reportValueCalculator));
            //Assert.Throws<ArgumentNullException>(() => new ExperimentSensor(experiment, subjectProvider, null));
        }




        [Theory, AutoDomainData]
        public void UpdateSensorValues_UsesSubjectQueryOnlyOnce_Test([Frozen] IExistingScienceSubjectProvider subjectProvider, ExperimentSensor sut) // to make sure we aren't being wasteful
        {
            sut.UpdateSensorValues();

            subjectProvider.Received(1).GetExistingSubject(Arg.Any<ScienceExperiment>(), Arg.Any<ExperimentSituations>(), Arg.Any<ICelestialBody>(), Arg.Any<string>());
        }


        [Theory, AutoDomainData]
        public void UpdateSensorValues_DoesNotReportChanged_WhenAllCalculatedValuesSame(
            [Frozen] ScienceExperiment experiment,
            IExistingScienceSubjectProvider subjectProvider, 
            IScienceSubject subject,
            IExperimentReportValueCalculator valueCalculator,
            float collectionValue,
            float transmissionValue,
            float labValue)
        {
            throw new NotImplementedException();
            //subjectProvider.GetExistingSubject(Arg.Any<ScienceExperiment>()).Returns(subject);

            //valueCalculator.CalculateRecoveryValue(Arg.Is(experiment), Arg.Is(subject))
            //    .Returns(collectionValue);

            //valueCalculator.CalculateTransmissionValue(Arg.Is(experiment), Arg.Is(subject))
            //    .Returns(transmissionValue);

            //valueCalculator.CalculateLabValue(Arg.Is(experiment), Arg.Is(subject)).Returns(labValue);


            //var sut = new ExperimentSensor(experiment, subjectProvider, valueCalculator);

            //sut.ClearChangedFlag();
            //sut.UpdateSensorValues(); // initially sets values

            //Assert.Equal(collectionValue, sut.RecoveryValue);
            //Assert.Equal(transmissionValue, sut.TransmissionValue);
            //Assert.Equal(labValue, sut.LabValue);

            //sut.ClearChangedFlag();
            //sut.UpdateSensorValues(); // shoudl result in no change
            //var result = sut.HasChanged;

            //Assert.False(result, "Incorrect status flag");


        }
    }
}
