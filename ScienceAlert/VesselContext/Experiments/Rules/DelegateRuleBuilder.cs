using strange.extensions.injector.api;

namespace ScienceAlert.VesselContext.Experiments.Rules
{
    [DoNotAutoRegister]
    class DelegateRuleBuilder : ObjectFromConfigNodeBuilderUsingFactoryDelegate<IExperimentRule, IRuleBuilder, IInjectionBinder, ITemporaryBindingFactory>, IRuleBuilder
    {
        public DelegateRuleBuilder(FactoryDelegate factoryFunc, params string[] handledConfigNames) : base(factoryFunc, handledConfigNames)
        {
        }
    }
}
