using System;
using System.Reflection;

namespace ScienceAlert.Core
{
    public class AssemblyNotFoundException : Exception
    {
        public AssemblyNotFoundException(Assembly assembly)
            : base("Failed to find Assembly \"" + assembly.FullName + "\" in AssemblyLoader")
        {
            
        }
    }
}
