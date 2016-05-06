using strange.extensions.injector.api;

namespace ScienceAlert.VesselContext.Experiments.Rules
{
    // just a marker to make finding the right builder a little easier when injecting
    interface IRuleFactory : IConfigNodeObjectBuilder<IExperimentRule, IRuleFactory, IInjectionBinder, ITemporaryBindingFactory>
    {
    }
}