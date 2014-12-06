using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ScienceAlert.Experiments.Sensors;
using ReeperCommon;

namespace ScienceAlert.Experiments
{
    static class SensorFactory
    {
        public static IExperimentSensor CreateSensor(string experimentid)
        {
            ScienceExperiment experiment = ResearchAndDevelopment.GetExperiment(experimentid);

            if (ReferenceEquals(experiment, null)) return null;

            switch (experimentid)
            {
                case "crewReport":
                    break;

                case "evaReport":
                    break;

                case "surfaceSample":
                    break;

                default:
                    break;
            }
        }
    }
}
