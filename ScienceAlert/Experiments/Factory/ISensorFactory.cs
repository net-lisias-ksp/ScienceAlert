using System.Collections.Generic;
using ScienceAlert.Experiments.Sensors;
using ScienceAlert.KSPInterfaces.PartModules;

namespace ScienceAlert.Experiments.Factory
{
    public interface ISensorFactory
    {
        IExperimentSensor Create(string experimentid, IEnumerable<IModuleScienceExperiment> allOnboardScienceModules);
    }
}
