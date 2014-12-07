using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ScienceAlert.KSPInterfaces.PartModules.Implementations
{
    class KspModuleScienceExperiment : IModuleScienceExperiment
    {
        private ModuleScienceExperiment _module;

        public KspModuleScienceExperiment(ModuleScienceExperiment kspModule)
        {
            _module = kspModule;
        }


        public void Deploy()
        {
            throw new NotImplementedException();
        }
    }
}
