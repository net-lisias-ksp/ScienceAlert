using ScienceAlert.Game;
using strange.extensions.promise.api;

namespace ScienceAlert.VesselContext.Experiments.Deployment
{
    public interface IDeploymentTrigger
    {
        IPromise Deploy(IVessel vessel, ScienceExperiment experiment);
        bool Busy { get; }
    }
}
