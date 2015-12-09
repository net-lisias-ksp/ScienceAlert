using System;

namespace ScienceAlert.Core.Gui
{
    public class ShaderNotFoundException : Exception
    {
        public ShaderNotFoundException(string shaderName)
            : base(shaderName + " not found")
        {
            
        }
    }
}
