using System.Collections.ObjectModel;
using ScienceAlert.VesselContext.Experiments.Sensors.Queries;

namespace ScienceAlert.Game
{
    public interface IVessel : IScienceContainerCollectionProvider,
                                ICelestialBodyProvider,
                                IExperimentSituationProvider,
                                IExperimentBiomeProvider
    {
        event Callback Modified;

        void Rescan();

        ReadOnlyCollection<ProtoCrewMember> EvaCapableCrew { get; }
        ReadOnlyCollection<IModuleScienceExperiment> ScienceExperimentModules { get; }

        bool IsControllable { get; }
        string VesselName { get; }
        Vessel.Situations VesselSituation { get; }

        double Latitude { get; }
        double Longitude { get; }
    }
}
