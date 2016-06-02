using System;
using strange.extensions.injector.api;

namespace ScienceAlert.VesselContext.Experiments.Sensors.Rules
{
    class CustomFactory : IRuleFactory
    {
        public ISensorRule Build(ConfigNode config, IRuleFactory parameter, IInjectionBinder binder, ITemporaryBindingFactory parameter2)
        {
            throw new NotImplementedException();
        }

        public bool CanHandle(ConfigNode config)
        {
            return false;
        }
    }
}
