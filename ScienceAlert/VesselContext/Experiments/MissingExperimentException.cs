using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ScienceAlert.VesselContext.Experiments
{
    public class MissingExperimentException : Exception
    {
        public MissingExperimentException(ScienceExperiment experiment) : base("Missing experiment: " + experiment.id + "; something is hosed")
        {
            
        }
    }
}
