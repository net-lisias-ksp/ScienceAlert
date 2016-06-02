using System;

namespace ScienceAlert.VesselContext.Experiments.Sensors.Trigger
{
    class NoSuitableEvaCandidateFoundException : Exception
    {
        public NoSuitableEvaCandidateFoundException(string message = "No suitable EVA candidate for the active vessel was found.") : base(message)
        {
            
        }
    }
}