using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ScienceAlert.Core
{
    public class ScenarioModuleNotFoundException : Exception
    {
        public ScenarioModuleNotFoundException(string name) : base("Failed to find ScenarioModule " + name)
        {
            
        }
    }
}
