using System;
using System.Text;

namespace ScienceAlert.VesselContext.Experiments.Rules
{
    public class TypeNotHandledByFactoryException : Exception
    {
        public TypeNotHandledByFactoryException(string typeName)
            : base("Type " + typeName + " is not handled by this factory")
        {
            
        }
    }
}
