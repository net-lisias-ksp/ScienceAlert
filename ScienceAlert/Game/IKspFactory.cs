using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ScienceAlert.Annotations;

namespace ScienceAlert.Game
{
    public interface IKspFactory
    {
        ILoadedAssembly Create([NotNull] AssemblyLoader.LoadedAssembly la);
        IVessel Create([NotNull] Vessel vessel);
    }
}
