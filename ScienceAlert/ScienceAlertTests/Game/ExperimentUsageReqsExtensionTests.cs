using ScienceAlert.Game;
using Xunit;
using Xunit.Extensions;

namespace ScienceAlertTests.Game
{
    public class ExperimentUsageReqsExtensionTests
    {
        [Theory]
        [InlineData(ExperimentUsageReqs.Never, ExperimentUsageReqs.CrewInPart, true)] // note to self: "Never" = -1, so all flags are set
        [InlineData(ExperimentUsageReqs.CrewInPart, ExperimentUsageReqs.CrewInPart, true)]
        [InlineData(ExperimentUsageReqs.ScientistCrew, ExperimentUsageReqs.CrewInPart, false)]
        [InlineData(ExperimentUsageReqs.CrewInPart, ExperimentUsageReqs.CrewInVessel, false)]
        [InlineData(ExperimentUsageReqs.CrewInPart | ExperimentUsageReqs.ScientistCrew, ExperimentUsageReqs.CrewInPart, true)]
        [InlineData(ExperimentUsageReqs.CrewInPart | ExperimentUsageReqs.ScientistCrew, ExperimentUsageReqs.ScientistCrew, true)]
        public void HasFlagTest(ExperimentUsageReqs toCheck, ExperimentUsageReqs flag, bool toCheckContainsFlag)
        {
            var result = toCheck.IsFlagSet(flag);

            Assert.Equal(toCheckContainsFlag, result);
        }
    }
}
