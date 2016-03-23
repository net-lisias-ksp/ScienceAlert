using ReeperCommon.Serialization;

namespace ScienceAlert.VesselContext.Experiments.Rules
{
    public class GenericRuleBuilder<TRuleType> : DefaultObjectFromConfigNodeBuilder<TRuleType, IExperimentRule, IRuleBuilder>, IRuleBuilder
        where TRuleType : IExperimentRule
    {
        public GenericRuleBuilder(IConfigNodeSerializer serializer) : base(serializer)
        {
        }
    }
}