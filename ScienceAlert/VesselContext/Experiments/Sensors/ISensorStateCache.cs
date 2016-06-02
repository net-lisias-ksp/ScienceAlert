using JetBrains.Annotations;
using ScienceAlert.UI;

namespace ScienceAlert.VesselContext.Experiments.Sensors
{
    public interface ISensorStateCache
    {
        ExperimentSensorState GetCachedState([NotNull] ScienceExperiment experiment);
        ExperimentSensorState GetCachedState([NotNull] IExperimentIdentifier identifier);
    }
}