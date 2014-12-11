using System;
using System.Collections.Generic;
using ScienceAlert.KSPInterfaces.PartModules;

namespace ScienceAlert.KSPInterfaces.FlightGlobals
{
    public interface IVessel
    {
        // ReSharper disable once InconsistentNaming
        string vesselName { get; }
        Part rootPart { get; }
        Guid id { get; }
        VesselType vesselType { get; }
        bool loaded { get; }
        IProtoVessel protoVessel { get; }

        List<Part> Parts { get; }

        IEnumerable<IModuleScienceExperiment> GetScienceExperimentModules();
        IEnumerable<IScienceDataContainer> GetScienceContainers();
        IEnumerable<IScienceDataTransmitter> GetTransmitters();
    }
}
