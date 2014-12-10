﻿using System;
using System.Collections.Generic;
using ScienceAlert.Experiments.Science;
using ScienceAlert.Experiments.Sensors;
using ScienceAlert.KSPInterfaces.PartModules;

namespace ScienceAlert.Experiments.Factory
{
    class SensorFactory : ISensorFactory
    {
        private readonly IStoredVesselScience _storedVesselScienceCache;


        public SensorFactory(IStoredVesselScience storedVesselData)
        {
            _storedVesselScienceCache = storedVesselData;
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
                    return new ExperimentSensor(experiment, ScienceAlertProfileManager.ActiveProfile.GetSensorSettings(experimentid), _storedVesselScienceCache, allOnboardScienceModules);
                    break;
            }
        }
    }
}