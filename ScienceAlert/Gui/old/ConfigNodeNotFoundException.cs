using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ScienceAlert.Gui
{
    public class ConfigNodeNotFoundException : Exception
    {
        public ConfigNodeNotFoundException(string name) : base("ConfigNode '" + name + "' not found")
        {
            
        }
    }
}
