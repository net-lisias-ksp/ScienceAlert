using System.Collections.Generic;
using strange.extensions.injector.api;

namespace ScienceAlert.VesselContext.Experiments
{
    public interface IExperimentSensorFactory
    {
        KeyValuePair<ISensor, ISensorValues> Create(ScienceExperiment experiment);
    }
}
