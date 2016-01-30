using System.Linq;
using ScienceAlert.VesselContext.Experiments.Rules;
using Xunit;
using Xunit.Extensions;

namespace ScienceAlertTests.Rules
{
    public class RuleDefinitionFactoryTests
    {
        [Theory, AutoDomainData]
        public void Create_WithSingleRule_Test(RuleDefinitionFactory sut)
        {
            var config = new ConfigNode(typeof(VesselHasModuleScienceExperiment).Name);

            var result = sut.Create(config);

            Assert.Equal(RuleDefinition.DefinitionType.Rule, result.Type);
            Assert.Empty(result.Rules);
            Assert.Same(config, result.RuleConfig.Value);
            Assert.Same(typeof (VesselHasModuleScienceExperiment), result.Rule.Value);
        }


        [Theory, AutoDomainData]
        public void Create_WithCompositeRuleset_Test(RuleDefinitionFactory sut)
        {
            var config = new ConfigNode(RuleDefinitionFactory.CompositeAllName);

            config.AddNode(new ConfigNode(typeof (VesselHasModuleScienceExperiment).Name));
            config.AddNode(new ConfigNode(typeof (VesselHasModuleScienceExperiment).Name));

            var result = sut.Create(config);

            Assert.Equal(RuleDefinition.DefinitionType.CompositeAll, result.Type);
            Assert.NotEmpty(result.Rules);
            Assert.Null(result.RuleConfig.Value);
            Assert.Contains(typeof (VesselHasModuleScienceExperiment), result.Rules.Select(n => n.Rule.Value));
            Assert.Equal(2, result.Rules.Count);
            Assert.True(result.Rules.All(r => r.Type == RuleDefinition.DefinitionType.Rule));
        }


        [Theory, AutoDomainData]
        public void Create_WithCompositeRuleset_ThatIncludesCompositeRuleset_Test(RuleDefinitionFactory sut)
        {
/*
 * ALL_OF
 * {
 *      VesselHasModuleScienceExperiment
 *      {
 *      }
 *      ALL_OF
 *      {
 *          VesselHasModuleScienceExperiment
 *          {
 *          }
 *      }
 * }
*/
            var config = new ConfigNode(RuleDefinitionFactory.CompositeAllName);

            config.AddNode(new ConfigNode(typeof (VesselHasModuleScienceExperiment).Name));

            config.AddNode(config.CreateCopy());

            var result = sut.Create(config);

            Assert.Equal(RuleDefinition.DefinitionType.CompositeAll, result.Type);
            Assert.Equal(2, result.Rules.Count);
            Assert.Equal(RuleDefinition.DefinitionType.Rule, result.Rules.First().Type);
            Assert.Equal(RuleDefinition.DefinitionType.CompositeAll, result.Rules.Last().Type);
            Assert.Equal(1, result.Rules.Last().Rules.Count);
        }
    }
}
