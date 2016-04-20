using strange.extensions.injector.api;

namespace ScienceAlert.VesselContext.Experiments.Trigger
{
    public interface ITriggerBuilder : IConfigNodeObjectBuilder<ExperimentTrigger, ITriggerBuilder, IInjectionBinder, ITemporaryBindingFactory>
    {
    }
}
