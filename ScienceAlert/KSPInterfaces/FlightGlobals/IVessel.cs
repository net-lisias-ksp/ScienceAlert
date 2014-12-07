using System.Collections.Generic;
using ScienceAlert.KSPInterfaces.PartModules;

namespace ScienceAlert.KSPInterfaces.FlightGlobals
{
    public interface IVessel
    {
        IEnumerable<IModuleScienceExperiment> GetScienceExperimentModules();
    }
}
