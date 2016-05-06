using strange.extensions.injector.api;

namespace ScienceAlert.VesselContext.Experiments.Trigger
{
    interface ITriggerFactory : IConfigNodeObjectBuilder<ExperimentTrigger, ITriggerFactory, IInjectionBinder, ITemporaryBindingFactory>
    {
    }
}
