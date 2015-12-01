using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ScienceAlert.Gui
{
    public class EmptyConfigNodeException : Exception
    {
        public EmptyConfigNodeException() : base("Specified ConfigNode does not contain any data")
        {
            
        }
    }
}
