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

        public KspVessel(Vessel vessel)
        {
            if (vessel.IsNull())
                throw new ArgumentNullException("vessel");
            _vessel = vessel;
        }


        public List<Part> Parts
        {
            get { return _vessel.Parts; }
        }

        public Part rootPart { get { return _vessel.rootPart;  }}

        public string vesselName { get { return _vessel.vesselName; }}

        public IEnumerable<IModuleScienceExperiment> GetScienceExperimentModules()
        {
            return
                _vessel.FindPartModulesImplementing<ModuleScienceExperiment>()
                    .Select(mse => new KspModuleScienceExperiment(mse))
                    .Cast<IModuleScienceExperiment>();
        }

        public IEnumerable<IScienceDataContainer> GetScienceContainers()
        {
            return
                _vessel.FindPartModulesImplementing<IScienceDataContainer>();
        }

        public IEnumerable<IScienceDataTransmitter> GetTransmitters()
        {
            return _vessel.FindPartModulesImplementing<IScienceDataTransmitter>();
        }
    }
}
