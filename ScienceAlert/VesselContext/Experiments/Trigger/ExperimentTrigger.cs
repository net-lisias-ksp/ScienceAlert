using System;
using JetBrains.Annotations;
using strange.extensions.promise.api;
using ScienceAlert.Game;

namespace ScienceAlert.VesselContext.Experiments.Trigger
{
    public abstract class ExperimentTrigger
    {
        protected readonly IVessel ActiveVessel;
        public ScienceExperiment Experiment { get; private set; }

        public ExperimentTrigger([NotNull] IVessel activeVessel, ScienceExperiment experiment)
        {
            if (experiment == null) throw new ArgumentNullException("experiment");
            if (activeVessel == null) throw new ArgumentNullException("activeVessel");
            ActiveVessel = activeVessel;
            Experiment = experiment;
        }

        public abstract IPromise Deploy();


        // Can the trigger be run?
        public abstract bool Busy { get; }
    }
}
