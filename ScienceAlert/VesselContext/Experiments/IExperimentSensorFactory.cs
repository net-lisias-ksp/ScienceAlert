using System.Collections.Generic;

namespace ScienceAlert.VesselContext.Experiments
{
    public interface IExperimentSensorFactory 
    {
        ISensor Create(ScienceExperiment experiment);
    }
}
