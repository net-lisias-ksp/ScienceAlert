using System.Collections.Generic;
using strange.extensions.injector.api;

namespace ScienceAlert.VesselContext.Experiments.Rules
{
    [DoNotAutoRegister]
    class CompositeRuleFactory : CompositeBuilder<IExperimentRule, IRuleFactory, IInjectionBinder, ITemporaryBindingFactory>,
        IRuleFactory
    {
        public CompositeRuleFactory(IEnumerable<IConfigNodeObjectBuilder<IExperimentRule, IRuleFactory, IInjectionBinder, ITemporaryBindingFactory>> builders)
            : base(builders)
        {
        }
    }
}