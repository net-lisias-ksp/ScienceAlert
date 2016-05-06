using System;
using ReeperKSP.Extensions;

namespace ScienceAlert
{
    class BuilderCannotHandleConfigNodeException : Exception
    {
        public BuilderCannotHandleConfigNodeException(ConfigNode config)
            : base("This builder cannot handle " + (config != null ? config.ToSafeString() : "<null ConfigNode>"))
        {
            
        }
    }
}