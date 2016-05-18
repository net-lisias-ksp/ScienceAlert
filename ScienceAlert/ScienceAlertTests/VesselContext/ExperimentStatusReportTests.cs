using Ploeh.AutoFixture.Xunit;
using ScienceAlert.VesselContext;
using Xunit;
using Xunit.Extensions;

namespace ScienceAlertTests.VesselContext
{
    public class ExperimentStatusReportTests
    {
        [Theory, AutoDomainData]
        public void Equals_Self_Test([Frozen] ExperimentSensorState report)
        {
            Assert.True(report.Equals(report));
        }

        [Theory, AutoDomainData]
        public void Equals_Test([Frozen] ExperimentSensorState report1, ExperimentSensorState report2)
        {
            Assert.True(report1.Equals(report2));
            Assert.True(report2.Equals(report1));
        }
    }
}
