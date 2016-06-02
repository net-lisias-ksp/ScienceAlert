using strange.extensions.injector.api;

namespace ScienceAlert.VesselContext.Experiments.Sensors.Rules
{
    [DoNotAutoRegister]
    class DelegateRuleFactory : ObjectFromConfigNodeBuilderUsingFactoryDelegate<ISensorRule, IRuleFactory, IInjectionBinder, ITemporaryBindingFactory>, IRuleFactory
    {
        public DelegateRuleFactory(FactoryDelegate factoryFunc, params string[] handledConfigNames) : base(factoryFunc, handledConfigNames)
        {
        }
    }
}
