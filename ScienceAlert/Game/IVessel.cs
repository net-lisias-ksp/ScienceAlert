using System.Collections.ObjectModel;


namespace ScienceAlert.Game
{
    public interface IVessel : IScienceContainerCollectionProvider,
                                ICelestialBodyProvider,
                                IExperimentSituationProvider,
                                IExperimentBiomeProvider,
                                IScienceDataTransmitterCollectionProvider,
                                IScienceExperimentModuleCollectionProvider,
                                IScienceLabCollectionProvider
    {
        event Callback Rescanned;

        ReadOnlyCollection<ProtoCrewMember> EvaCapableCrew { get; }

        bool IsControllable { get; }
        string VesselName { get; }
        Vessel.Situations VesselSituation { get; }

        double Latitude { get; }
        double Longitude { get; }

        bool Landed { get; }
        bool SplashedDown { get; }
    }
}
