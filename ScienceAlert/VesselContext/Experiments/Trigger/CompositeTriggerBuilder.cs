using System.Collections.Generic;
using strange.extensions.injector.api;

namespace ScienceAlert.VesselContext.Experiments.Trigger
{
    [DoNotAutoRegister]
    class CompositeTriggerBuilder : CompositeBuilder<ExperimentTrigger, ITriggerBuilder, IInjectionBinder, ITemporaryBindingFactory>
    {
        public CompositeTriggerBuilder(IEnumerable<IConfigNodeObjectBuilder<ExperimentTrigger, ITriggerBuilder, IInjectionBinder, ITemporaryBindingFactory>> builders) : base(builders)
        {
        }
    }
}
