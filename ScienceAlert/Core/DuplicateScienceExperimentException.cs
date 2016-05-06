using System;

namespace ScienceAlert.Core
{
    class DuplicateScienceExperimentException : Exception
    {
        public DuplicateScienceExperimentException(ScienceExperiment exp)
            : base("A binding for " + exp.id + " already exists!")
        {
            
        }
    }
}
