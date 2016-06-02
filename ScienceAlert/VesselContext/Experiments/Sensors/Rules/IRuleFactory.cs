using strange.extensions.injector.api;

namespace ScienceAlert.VesselContext.Experiments.Sensors.Rules
{
    // just a marker to make finding the right builder a little easier when injecting
    interface IRuleFactory : IConfigNodeObjectBuilder<ISensorRule, IRuleFactory, IInjectionBinder, ITemporaryBindingFactory>
    {
    }
}