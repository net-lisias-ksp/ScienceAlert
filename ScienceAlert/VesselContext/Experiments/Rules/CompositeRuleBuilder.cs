using System.Collections.Generic;
using strange.extensions.injector.api;

namespace ScienceAlert.VesselContext.Experiments.Rules
{
    [ExcludeFromConventionalRegistration]
    public class CompositeRuleBuilder : CompositeBuilder<IExperimentRule, IRuleBuilder, IInjectionBinder, ITemporaryBindingFactory>,
        IRuleBuilder
    {
        public CompositeRuleBuilder(IEnumerable<IConfigNodeObjectBuilder<IExperimentRule, IRuleBuilder, IInjectionBinder, ITemporaryBindingFactory>> builders)
            : base(builders)
        {
        }
    }
}