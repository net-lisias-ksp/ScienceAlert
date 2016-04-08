using ReeperCommon.Serialization;
using strange.extensions.injector.api;

namespace ScienceAlert.VesselContext.Experiments.Rules
{
    [ExcludeFromConventionalRegistration]
    public class GenericRuleBuilder<TRuleType> : DefaultObjectFromConfigNodeBuilder<TRuleType, IExperimentRule, IInjectionBinder, IRuleBuilder>, IRuleBuilder
        where TRuleType : IExperimentRule
    {
        public GenericRuleBuilder(IConfigNodeSerializer serializer) : base(serializer)
        {
        }
    }
}