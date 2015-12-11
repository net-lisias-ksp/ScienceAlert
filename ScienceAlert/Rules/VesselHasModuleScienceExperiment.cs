using System;

namespace ScienceAlert.Rules
{
    public class VesselHasModuleScienceExperiment : IExperimentRule
    {
        private readonly ScienceExperiment _experiment;

        public VesselHasModuleScienceExperiment(ScienceExperiment experiment)
        {
            if (experiment == null) throw new ArgumentNullException("experiment");
            _experiment = experiment;
        }


        public bool Get()
        {
            throw new NotImplementedException();
        }
    }
}
