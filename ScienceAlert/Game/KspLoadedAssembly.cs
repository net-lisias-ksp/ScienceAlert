using System;
using System.Reflection;
using ScienceAlert.Annotations;

namespace ScienceAlert.Game
{
    public class KspLoadedAssembly : ILoadedAssembly
    {
        private readonly AssemblyLoader.LoadedAssembly _targetLoadedAssembly;


        public KspLoadedAssembly([NotNull] AssemblyLoader.LoadedAssembly targetLoadedAssembly)
        {
            if (targetLoadedAssembly == null) throw new ArgumentNullException("targetLoadedAssembly");
            _targetLoadedAssembly = targetLoadedAssembly;
        }

        public Assembly Assembly
        {
            get { return _targetLoadedAssembly.assembly; }
        }
    }
}
