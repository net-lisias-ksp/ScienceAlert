namespace ScienceAlert.VesselContext.Experiments
{
    public interface IExperimentObserverFactory
    {
        IExperimentObserver Create(ScienceExperiment experiment);
    }
}
