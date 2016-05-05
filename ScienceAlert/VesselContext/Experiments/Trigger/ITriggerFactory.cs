using strange.extensions.injector.api;

namespace ScienceAlert.VesselContext.Experiments.Trigger
{
    public interface ITriggerFactory : IConfigNodeObjectBuilder<ExperimentTrigger, ITriggerFactory, IInjectionBinder, ITemporaryBindingFactory>
    {
    }
}
