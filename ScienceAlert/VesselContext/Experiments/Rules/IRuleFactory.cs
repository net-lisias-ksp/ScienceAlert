using ReeperCommon.Serialization;
using strange.extensions.injector.api;

namespace ScienceAlert.VesselContext.Experiments.Rules
{
    public interface IRuleFactory
    {
        IExperimentRule Create(IInjectionBinder context, IConfigNodeSerializer serializer);
    }
}
