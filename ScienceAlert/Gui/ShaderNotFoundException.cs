using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ScienceAlert.Gui
{
    public class ShaderNotFoundException : Exception
    {
        public ShaderNotFoundException(string shaderName)
            : base(shaderName + " not found")
        {
            
        }
    }
}
