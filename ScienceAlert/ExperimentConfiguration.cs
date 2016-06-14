using System;
using ReeperCommon.Containers;

// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace ScienceAlert
{
    public class ExperimentConfiguration 
    {
        public ScienceExperiment Experiment { get; private set; }

        public Maybe<ConfigNode> SensorDefinition { get; private set; } // uses default if none
        public Maybe<string> TriggerDefinition { get; private set; } // uses default if none

        public ExperimentConfiguration(
            ScienceExperiment experiment, Maybe<ConfigNode> sensorDef, Maybe<string> triggerDef)
        {
            if (experiment == null) throw new ArgumentNullException("experiment");

            Experiment = experiment;
            SensorDefinition = sensorDef;
            TriggerDefinition = triggerDef;
        }


        public override string ToString()
        {
            return typeof (ExperimentConfiguration).Name + " (experiment: " + Experiment.id + ")";
        }
    }
}
