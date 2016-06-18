using System;

namespace ScienceAlert.VesselContext.Experiments.Triggers
{
    public class NoSuitableEvaCandidateFoundException : Exception
    {
        public NoSuitableEvaCandidateFoundException() : base("Could not find a suitable EVA candidate")
        {
            
        }
    }
}