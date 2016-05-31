using ScienceAlert.UI;

namespace ScienceAlert.VesselContext.Experiments
{
    public interface IAlertStateCache
    {
        ExperimentAlertStatus GetStatus(IExperimentIdentifier identifier);
    }
}
