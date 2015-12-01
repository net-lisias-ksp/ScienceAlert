using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ScienceAlert.Core
{
    public class TextureNotFoundException : Exception
    {
        public TextureNotFoundException(string url) : base("Texture URL \"" + url + "\" not found")
        {
            
        }
    }
}
