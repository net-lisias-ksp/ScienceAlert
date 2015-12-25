using System;

namespace ScienceAlert.VesselContext.Experiments.Rules
{
    public class RuleMustImplementCorrectInterfaceException : Exception
    {
        public RuleMustImplementCorrectInterfaceException(Type badType)
            : base(badType.FullName + " must implement the correct interface to work as a rule type")
        {
            
        }
    }
}
