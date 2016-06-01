using System;

namespace ScienceAlert
{
    class BuilderConstructedTypeCannotBeAssignedToReturnTypeException : Exception
    {
        public BuilderConstructedTypeCannotBeAssignedToReturnTypeException(Type builderType, Type returnType)
            : base(builderType.FullName + " cannot be assigned to return type " + returnType.FullName)
        {
            
        }
    }
}