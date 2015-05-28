using System;

namespace ScienceAlert.Game
{
    public class KspFactory : IKspFactory
    {
        public ILoadedAssembly Create(AssemblyLoader.LoadedAssembly la)
        {
            if (la == null) throw new ArgumentNullException("la");
            return new KspLoadedAssembly(la);
        }
    }
}
