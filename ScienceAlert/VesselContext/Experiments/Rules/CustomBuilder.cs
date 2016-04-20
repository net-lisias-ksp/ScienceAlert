using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using strange.extensions.injector.api;

namespace ScienceAlert.VesselContext.Experiments.Rules
{
    class CustomBuilder : IRuleBuilder
    {
        public IExperimentRule Build(ConfigNode config, IRuleBuilder parameter, IInjectionBinder binder, ITemporaryBindingFactory parameter2)
        {
            throw new NotImplementedException();
        }

        public bool CanHandle(ConfigNode config)
        {
            return false;
        }
    }
}
