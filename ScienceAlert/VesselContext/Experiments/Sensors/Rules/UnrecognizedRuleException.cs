using System;

namespace ScienceAlert.VesselContext.Experiments.Sensors.Rules
{
    public class UnrecognizedRuleException : Exception
    {
        public UnrecognizedRuleException(string typeName) : base("Unrecognized rule: " + typeName)
        {
            
        }
    }
}