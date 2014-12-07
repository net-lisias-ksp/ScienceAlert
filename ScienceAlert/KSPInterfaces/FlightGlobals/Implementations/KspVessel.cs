using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ScienceAlert.KSPInterfaces.PartModules;
using ScienceAlert.KSPInterfaces.PartModules.Implementations;

namespace ScienceAlert.KSPInterfaces.FlightGlobals.Implementations
{
    class KspVessel : IVessel
    {
        private readonly Vessel _vessel;

        public KspVessel(Vessel v)
        {
            _vessel = v;
        }


        public IEnumerable<IModuleScienceExperiment> GetScienceExperimentModules()
        {
            return
                _vessel.FindPartModulesImplementing<ModuleScienceExperiment>()
                    .Select(mse => new KspModuleScienceExperiment(mse))
                    .Cast<IModuleScienceExperiment>();
        }
    }
}
