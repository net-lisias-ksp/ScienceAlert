using System;

namespace ScienceAlert.Rules
{
    public class DuplicateConfigNodeSectionException : Exception
    {
        public DuplicateConfigNodeSectionException(string sectionName)
            : base("Specified ConfigNode has multiple \"" + sectionName + "\" sections which is not allowed")
        {
            
        }
    }
}
