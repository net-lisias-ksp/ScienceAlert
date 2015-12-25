using System.Collections.ObjectModel;

namespace ScienceAlert.Game
{
    public interface IVessel
    {
        event Callback Modified;

        void Rescan();

        ReadOnlyCollection<ProtoCrewMember> EvaCapableCrew { get; }
        ReadOnlyCollection<IModuleScienceExperiment> ScienceExperimentModules { get; }

        bool IsControllable { get; }
        string VesselName { get; }
        Vessel.Situations VesselSituation { get; }
        ExperimentSituations ExperimentSituation { get; }
        CelestialBody Body { get; }
    }
}
