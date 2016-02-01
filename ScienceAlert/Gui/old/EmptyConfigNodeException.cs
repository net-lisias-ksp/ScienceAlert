using System;

namespace ScienceAlert.Gui.old
{
    public class EmptyConfigNodeException : Exception
    {
        public EmptyConfigNodeException() : base("Specified ConfigNode does not contain any data")
        {
            
        }
    }
}
