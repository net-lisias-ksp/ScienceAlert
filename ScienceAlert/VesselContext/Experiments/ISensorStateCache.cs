using JetBrains.Annotations;
using ScienceAlert.UI;

namespace ScienceAlert.VesselContext.Experiments
{
    public interface ISensorStateCache
    {
        ExperimentSensorState GetCachedState([NotNull] ScienceExperiment experiment);
        ExperimentSensorState GetCachedState([NotNull] IExperimentIdentifier identifier);
    }
}