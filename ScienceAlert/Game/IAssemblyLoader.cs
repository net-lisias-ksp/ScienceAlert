using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using ReeperCommon.Containers;

namespace ScienceAlert.Game
{
    public interface IAssemblyLoader
    {
        IEnumerable<ILoadedAssembly> LoadedAssemblies { get; }
        Maybe<ILoadedAssembly> GetByAssembly(Assembly assembly); 
        void Remove(ILoadedAssembly la);
    }
}
