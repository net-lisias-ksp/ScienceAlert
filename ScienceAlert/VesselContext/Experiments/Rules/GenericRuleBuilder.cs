using ReeperKSP.Serialization;
using strange.extensions.injector.api;

namespace ScienceAlert.VesselContext.Experiments.Rules
{
    [DoNotAutoRegister]
    public class GenericRuleBuilder<TRuleType> : DefaultObjectFromConfigNodeBuilder<TRuleType, IExperimentRule, IRuleBuilder, IInjectionBinder>, IRuleBuilder
        where TRuleType : IExperimentRule
    {
        public GenericRuleBuilder(IConfigNodeSerializer serializer) : base(serializer)
        {
        }
    }
}