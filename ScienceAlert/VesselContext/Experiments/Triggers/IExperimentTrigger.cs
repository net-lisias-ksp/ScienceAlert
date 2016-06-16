using strange.extensions.promise.api;

namespace ScienceAlert.VesselContext.Experiments.Triggers
{
    public interface IExperimentTrigger
    {
        IPromise Deploy();
        bool IsBusy { get; }
    }
}
