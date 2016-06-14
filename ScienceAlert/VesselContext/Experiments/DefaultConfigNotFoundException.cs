using System;

namespace ScienceAlert.VesselContext.Experiments
{
    class DefaultConfigNotFoundException : Exception
    {
        public DefaultConfigNotFoundException(string defaultNodeName)
            : base("Could not find default ConfigNode '" + defaultNodeName + "'")
        {
            
        }
    }
}