using ReeperKSP.Serialization;
using strange.extensions.injector.api;

namespace ScienceAlert.VesselContext.Experiments.Rules
{
    [DoNotAutoRegister]
    class GenericRuleFactory<TRuleType> : DefaultObjectFromConfigNodeBuilder<TRuleType, IExperimentRule, IRuleFactory, IInjectionBinder>, IRuleFactory
        where TRuleType : IExperimentRule
    {
        public GenericRuleFactory(IConfigNodeSerializer serializer) : base(serializer)
        {
        }
    }
}