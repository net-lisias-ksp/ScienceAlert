using System.Collections.ObjectModel;

namespace ScienceAlert.Game
{
    public interface ICrewContainer
    {
        ReadOnlyCollection<ProtoCrewMember> EvaCapableCrew { get; }
        ReadOnlyCollection<ProtoCrewMember> AllCrew { get; }
        bool HasScientist { get; }
    }

    public interface IVessel : IScienceContainerCollectionProvider,
                                ICelestialBodyProvider,
                                IExperimentSituationProvider,
                                IExperimentBiomeProvider,
                                IScienceExperimentModuleCollectionProvider,
                                IScienceLabCollectionProvider,
                                ICrewContainer
    {
        event Callback Rescanned;

        ReadOnlyCollection<Part> Parts { get; }

        bool IsControllable { get; }
        string VesselName { get; }
        Vessel.Situations VesselSituation { get; }

        double Latitude { get; }
        double Longitude { get; }

        bool Landed { get; }
        bool SplashedDown { get; }

        bool isEVA { get; }
    }
}
