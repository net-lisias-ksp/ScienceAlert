using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ScienceAlert.Experiments.Sensors;
using ScienceAlert.Experiments.Science;
using ReeperCommon;

namespace ScienceAlert.Experiments
{
    using ProfileManager = ScienceAlertProfileManager;

    class SensorFactory
    {
        private readonly StoredVesselScience _storedVesselScienceCache;


        public SensorFactory(StoredVesselScience storedVesselData)
        {
            _storedVesselScienceCache = storedVesselData;
        }


        public IExperimentSensor Create(string experimentid, IEnumerable<ModuleScienceExperiment> allOnboardScienceModules)
        {
            if (allOnboardScienceModules == null) allOnboardScienceModules = new List<ModuleScienceExperiment>();

            ScienceExperiment experiment = ResearchAndDevelopment.GetExperiment(experimentid);
            if (ReferenceEquals(experiment, null)) return null;

            switch (experimentid)
            {
                case "crewReport":
                    throw new NotImplementedException();
                    break;

                case "evaReport":
                    throw new NotImplementedException();
                    break;

                case "surfaceSample":
                    throw new NotImplementedException();
                    break;

                default:
                    return new ExperimentSensor(experiment, ProfileManager.ActiveProfile[experimentid], _storedVesselScienceCache, allOnboardScienceModules);
                    break;
            }
        }
    }
}
