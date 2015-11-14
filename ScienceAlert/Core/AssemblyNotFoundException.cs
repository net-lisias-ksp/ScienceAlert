using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

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
