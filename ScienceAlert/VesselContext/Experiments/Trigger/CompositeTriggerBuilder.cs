using System.Collections.Generic;

namespace ScienceAlert.VesselContext.Experiments.Trigger
{
    [ExcludeFromConventionalRegistration]
    class CompositeTriggerBuilder : CompositeBuilder<ExperimentTrigger, ITriggerBuilder, ITemporaryBindingFactory>
    {
        public CompositeTriggerBuilder(IEnumerable<IConfigNodeObjectBuilder<ExperimentTrigger, ITriggerBuilder, ITemporaryBindingFactory>> builders) : base(builders)
        {
        }
    }
}
