using System;

namespace ScienceAlert.Core.Gui
{
    public class TextureNotFoundException : Exception
    {
        public TextureNotFoundException(string url) : base("Texture URL \"" + url + "\" not found")
        {
            
        }
    }
}
