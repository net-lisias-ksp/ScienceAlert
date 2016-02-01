using System;

namespace ScienceAlert.VesselContext.Experiments.Rules
{
    public class FailedToCreateRuleException : Exception
    {
        public FailedToCreateRuleException() : base("Could not construct the specified rule")
        {
            
        }

        public FailedToCreateRuleException(Type targetType)
            : base(
                "Failed to construct rule of type " + targetType.FullName)
        {
            
        }

        public FailedToCreateRuleException(string message, Exception inner) : base(message, inner)
        {
            
        }
    }
}
