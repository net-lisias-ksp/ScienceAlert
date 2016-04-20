using System;

namespace ScienceAlert
{
    class DependenciesCannotBeSatisfiedException : Exception
    {
        public DependenciesCannotBeSatisfiedException(Type constructedType)
            : base(
                constructedType.FullName + " cannot be created because one or more dependencies cannot be satisfied:")
        {
            
        }
    }
}