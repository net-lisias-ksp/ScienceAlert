using ReeperKSP.Serialization;
using strange.extensions.injector.api;

namespace ScienceAlert.VesselContext.Experiments.Sensors.Rules
{
    /// <summary>
    /// If a rule does not have a custom rule builder, this one will be used. It will attempt to fill any dependencies using the IOC framework
    /// </summary>
    /// <typeparam name="TRuleType"></typeparam>
    [DoNotAutoRegister]
    class GenericRuleFactory<TRuleType> : DefaultObjectFromConfigNodeBuilder<TRuleType, ISensorRule, IRuleFactory, IInjectionBinder>, IRuleFactory
        where TRuleType : ISensorRule
    {
        public GenericRuleFactory(IConfigNodeSerializer serializer) : base(serializer)
        {
        }
    }
}