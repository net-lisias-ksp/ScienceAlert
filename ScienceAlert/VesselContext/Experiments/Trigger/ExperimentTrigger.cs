using System;
using strange.extensions.promise.api;

namespace ScienceAlert.VesselContext.Experiments.Trigger
{
    public abstract class ExperimentTrigger
    {
        public ScienceExperiment Experiment { get; private set; }

        public ExperimentTrigger(ScienceExperiment experiment)
        {
            if (experiment == null) throw new ArgumentNullException("experiment");
            Experiment = experiment;
        }

        public abstract IPromise Deploy();
        public abstract bool Busy { get; }
    }
}
