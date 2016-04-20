using System;

namespace ScienceAlert
{
    class CouldNotCreateInstanceException : Exception
    {
        public CouldNotCreateInstanceException(Type constructedType)
            : base("Could not create instance of " + constructedType.FullName)
        {
            
        }
    }
}