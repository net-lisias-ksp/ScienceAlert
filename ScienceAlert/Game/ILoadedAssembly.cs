using System.Reflection;

namespace ScienceAlert.Game
{
    public interface ILoadedAssembly
    {
        Assembly Assembly { get; }
    }
}
