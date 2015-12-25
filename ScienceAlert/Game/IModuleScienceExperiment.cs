namespace ScienceAlert.Game
{
    public interface IModuleScienceExperiment
    {
        IPart part { get; }

        bool Deployed { get; }
        string experimentID { get; }
        ExperimentUsageReqs InternalUsageRequirements { get; }
    }
}
