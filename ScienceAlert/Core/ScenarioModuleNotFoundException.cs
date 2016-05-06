using System;

namespace ScienceAlert.Core
{
    class ScenarioModuleNotFoundException : Exception
    {
        public ScenarioModuleNotFoundException(string name) : base("Failed to find ScenarioModule " + name)
        {
            
        }
    }
}
