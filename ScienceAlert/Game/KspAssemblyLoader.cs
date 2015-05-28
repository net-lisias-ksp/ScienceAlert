using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using ReeperCommon.Containers;
using ScienceAlert.Annotations;

namespace ScienceAlert.Game
{
    public class KspAssemblyLoader : IAssemblyLoader
    {
        private readonly IKspFactory _kspFactory;

        public KspAssemblyLoader([NotNull] IKspFactory kspFactory)
        {
            if (kspFactory == null) throw new ArgumentNullException("kspFactory");
            _kspFactory = kspFactory;
        }


        public IEnumerable<ILoadedAssembly> LoadedAssemblies
        {
            get { return AssemblyLoader.loadedAssemblies.Select(la => _kspFactory.Create(la)); }
        }


        public Maybe<ILoadedAssembly> GetByAssembly([NotNull] Assembly assembly)
        {
            if (assembly == null) throw new ArgumentNullException("assembly");

            var found = AssemblyLoader.loadedAssemblies.GetByAssembly(assembly);

            return found != null ? Maybe<ILoadedAssembly>.With(_kspFactory.Create(found)) : Maybe<ILoadedAssembly>.None;
        }


        public void Remove([NotNull] ILoadedAssembly la)
        {
            if (la == null) throw new ArgumentNullException("la");

            var target = AssemblyLoader.loadedAssemblies.GetByAssembly(la.Assembly);

            if (target == null)
                throw new InvalidOperationException("Did not find assembly " + la.Assembly.GetName().Name +
                                                " in AssemblyLoader");

            for (int i = 0; i < AssemblyLoader.loadedAssemblies.Count; ++i)
            {
                if (ReferenceEquals(AssemblyLoader.loadedAssemblies[i], target))
                {
                    AssemblyLoader.loadedAssemblies.RemoveAt(i);
                    return;
                }
            }

            throw new Exception("Failed to find target LoadedAssembly");
        }
    }
}
