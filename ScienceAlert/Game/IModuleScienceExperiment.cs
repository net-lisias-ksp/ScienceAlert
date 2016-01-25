namespace ScienceAlert.Game
{
    public interface IModuleScienceExperiment
    {
        IPart Part { get; }

        bool Deployed { get; }
// ReSharper disable once InconsistentNaming
        string ExperimentID { get; }
        ExperimentUsageReqs InternalUsageRequirements { get; }

        bool CanBeDeployed { get; }
        float TransmissionMultiplier { get; }
    }
}
