namespace ScienceAlert.KSPInterfaces.PartModules
{
    public interface IModuleScienceExperiment
    {
        float xmitDataScalar { get; }
        bool Deployed { get; }
        bool Inoperable { get; }

        void Deploy();
    }
}
