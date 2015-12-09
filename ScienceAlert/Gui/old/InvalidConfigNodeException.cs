using System;

namespace ScienceAlert.Gui
{
    public class InvalidConfigNodeException : Exception
    {
        public InvalidConfigNodeException(string configPath)
            : base("The ConfigNode at \"" + configPath + "\" is invalid")
        {
            
        }
    }
}
