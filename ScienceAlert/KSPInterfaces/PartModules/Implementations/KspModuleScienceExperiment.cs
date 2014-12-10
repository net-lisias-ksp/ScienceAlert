using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ScienceAlert.KSPInterfaces.PartModules.Implementations
{
    class KspModuleScienceExperiment : IModuleScienceExperiment
    {
        private readonly ModuleScienceExperiment _module;

        public KspModuleScienceExperiment(ModuleScienceExperiment kspModule)
        {
            if (_module.IsNull())
                throw new ArgumentNullException("kspModule");

            _module = kspModule;
        }


        



        public float xmitDataScalar
        {
            get { return _module.xmitDataScalar; }
        }

        public bool Deployed { get { return _module.Deployed; }}
        public bool Inoperable { get { return _module.Inoperable; }}


        public void Deploy()
        {
            throw new NotImplementedException();
        }
    }
}
