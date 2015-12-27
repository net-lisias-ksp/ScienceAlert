using System.Collections.Generic;
using ScienceAlert.VesselContext.Experiments.Sensors;

namespace ScienceAlert.VesselContext.Experiments
{
    public interface IExperimentSensorFactory 
    {
        ISensor Create(ScienceExperiment experiment);
    }
}
