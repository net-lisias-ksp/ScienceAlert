using System;

namespace ScienceAlert.VesselContext.Experiments.Rules
{
    public class ConfigNodeNotFoundException : Exception
    {
        public ConfigNodeNotFoundException(string name) : base("ConfigNode '" + name + "' not found")
        {
            
        }
    }
}
