using System;

namespace ScienceAlert.Core
{
    public class ScenarioModuleNotFoundException : Exception
    {
        public ScenarioModuleNotFoundException(string name) : base("Failed to find ScenarioModule " + name)
        {
            
        }
    }
}
