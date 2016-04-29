using System.Collections.Generic;
using strange.extensions.injector.api;

namespace ScienceAlert.VesselContext.Experiments.Trigger
{
    [DoNotAutoRegister]
    class CompositeTriggerBuilder : CompositeBuilder<ExperimentTrigger, ITriggerBuilder, IInjectionBinder, ITemporaryBindingFactory>, ITriggerBuilder
    {
        public CompositeTriggerBuilder(IList<IConfigNodeObjectBuilder<ExperimentTrigger, ITriggerBuilder, IInjectionBinder, ITemporaryBindingFactory>> builders)
            : base(builders)
        {
        }
    }
}
