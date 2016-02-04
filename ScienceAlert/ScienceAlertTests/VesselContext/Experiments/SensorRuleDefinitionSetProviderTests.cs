using System.Linq;
using System.Runtime.InteropServices;
using NSubstitute;
using ReeperCommon.Containers;
using ReeperCommon.Extensions;
using ScienceAlert.Game;
using ScienceAlert.VesselContext.Experiments;
using Xunit;
using Xunit.Extensions;

namespace ScienceAlertTests.VesselContext.Experiments
{
    public class SensorRuleDefinitionSetProviderTests
    {
        //public static class GameDatabaseBuilder
        //{
        //    public static IGameDatabase Build()
        //    {

        //        var gameDatabase = Substitute.For<IGameDatabase>();

        //    }
        //}

        [Fact()]
        public void SensorRuleDefinitionSetProviderTest()
        {
            Assert.True(false, "not implemented yet");
        }

        [Fact()]
        public void GetDefinitionSetTest()
        {
            Assert.True(false, "not implemented yet");
        }

        [Fact()]
        public void GetDefaultDefinitionTest()
        {
            Assert.True(false, "not implemented yet");
        }


        [Theory, AutoDomainData]
        public void GetDefaultDefinition_UsesDefaults(
            IGameDatabase gameDatabase, 
            IUrlConfig gameDatabaseValueOnDefaultNodeName, 
            IUrlConfig gameDatabaseValueOnOtherName, 
            string expectedNodeName, string failNodeName)
        {
            var nonDefaultConfig = new ConfigNode(failNodeName);
            var config = new ConfigNode(expectedNodeName);

            gameDatabaseValueOnDefaultNodeName.Config.Returns(config);
            gameDatabaseValueOnOtherName.Config.Returns(nonDefaultConfig);

            gameDatabase.GetConfigs(Arg.Any<string>()).Returns(
                ci =>
                {
                    var arg = ci.Arg<string>();

                    if (arg == SensorRuleDefinitionSetProvider.DefaultOnboardRuleNodeName ||
                        arg == SensorRuleDefinitionSetProvider.DefaultAvailabilityRuleNodeName ||
                        arg == SensorRuleDefinitionSetProvider.DefaultConditionRuleNodeName)
                        return new[] {gameDatabaseValueOnDefaultNodeName};

                    return new[] {gameDatabaseValueOnOtherName};
                });

            var sut = new SensorRuleDefinitionSetProvider(gameDatabase);
            var result = sut.GetDefaultDefinition();

            Assert.Same(config, result.OnboardDefinition);
            Assert.Same(config, result.AvailabilityDefinition);
            Assert.Same(config, result.ConditionDefinition);
        }
        

        [Theory, AutoDomainData]
        public void CreateRulesetFor_WithEmptyNodeName_BuildsUsingDefaults(
            IGameDatabase gameDatabase,
            IUrlConfig defaultUrlConfig,
            string valueName, string value, string subNode)
        {
            var defaultConfig = new ConfigNode("default");
            defaultConfig.AddValue(valueName, value);
            defaultConfig.AddNode(subNode);

            defaultUrlConfig.Config.Returns(defaultConfig);

            gameDatabase.GetConfigs(Arg.Any<string>()).Returns(new [] {defaultUrlConfig });

            var expected = new ConfigNode(SensorRuleDefinitionSetProvider.ExperimentRuleConfigNodeName);
            expected.AddNode(SensorRuleDefinitionSetProvider.OnboardRuleNodeName);
            expected.AddNode(SensorRuleDefinitionSetProvider.AvailabilityRuleNodeName);
            expected.AddNode(SensorRuleDefinitionSetProvider.ConditionRuleNodeName);

            var sut = new SensorRuleDefinitionSetProvider(gameDatabase);
            var result = sut.CreateRulesetFor(Maybe<ConfigNode>.None);

            Assert.Same(SensorRuleDefinitionSetProvider.OnboardRuleNodeName, result.OnboardDefinition.name);
            Assert.Same(SensorRuleDefinitionSetProvider.AvailabilityRuleNodeName, result.AvailabilityDefinition.name);
            Assert.Same(SensorRuleDefinitionSetProvider.ConditionRuleNodeName, result.ConditionDefinition.name);

            Assert.True(result.OnboardDefinition.MatchesContentsOf(defaultConfig, true));
            Assert.True(result.AvailabilityDefinition.MatchesContentsOf(defaultConfig, true));
            Assert.True(result.ConditionDefinition.MatchesContentsOf(defaultConfig, true));
        }
    }
}
