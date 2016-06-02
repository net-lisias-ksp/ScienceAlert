using System.Collections.Generic;
using strange.extensions.injector.api;

namespace ScienceAlert.VesselContext.Experiments.Sensors.Rules
{
    [DoNotAutoRegister]
    class CompositeRuleFactory : CompositeBuilder<ISensorRule, IRuleFactory, IInjectionBinder, ITemporaryBindingFactory>,
        IRuleFactory
    {
        public CompositeRuleFactory(IEnumerable<IConfigNodeObjectBuilder<ISensorRule, IRuleFactory, IInjectionBinder, ITemporaryBindingFactory>> builders)
            : base(builders)
        {
        }
    }
}