using System;
using System.Collections.Generic;
using ScienceAlert.Experiments.Science;
using ScienceAlert.Experiments.Sensors;
using ScienceAlert.KSPInterfaces.PartModules;
using ScienceAlert.ProfileData;

namespace ScienceAlert.Experiments.Factory
{
    class SensorFactory : ISensorFactory
    {
        private readonly IStoredVesselScience _storedVesselScienceCache;
        private readonly BiomeFilter _biomeFilter;
        private readonly IProfileManager _profileManager;

        public SensorFactory(IProfileManager profileManager, IStoredVesselScience storedVesselData, BiomeFilter biomeFilter)
        {
            if (profileManager.IsNull())
                throw new ArgumentNullException("profileManager");
            if (storedVesselData.IsNull())
                throw new ArgumentNullException("storedVesselData");
            if (biomeFilter.IsNull())
                throw new ArgumentNullException("biomeFilter");

            _profileManager = profileManager;
            _storedVesselScienceCache = storedVesselData;
            _biomeFilter = biomeFilter;
        }



        public IExperimentSensor Create(string experimentid, IEnumerable<IModuleScienceExperiment> allOnboardScienceModules)
        {
            if (allOnboardScienceModules == null) allOnboardScienceModules = new List<IModuleScienceExperiment>();

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
                    return new ExperimentSensor(experiment, _profileManager.ActiveProfile.GetSensorSettings(experimentid), _biomeFilter, _storedVesselScienceCache, allOnboardScienceModules);
                    break;
            }
        }
    }
}
