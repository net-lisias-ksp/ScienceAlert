using ReeperKSP.Serialization;
using strange.extensions.injector.api;

namespace ScienceAlert.VesselContext.Experiments.Rules
{
    [DoNotAutoRegister]
    class GenericRuleFactory<TRuleType> : DefaultObjectFromConfigNodeBuilder<TRuleType, ISensorRule, IRuleFactory, IInjectionBinder>, IRuleFactory
        where TRuleType : ISensorRule
    {
        public GenericRuleFactory(IConfigNodeSerializer serializer) : base(serializer)
        {
        }
    }
}