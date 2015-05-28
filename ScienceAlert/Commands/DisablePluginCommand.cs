using System;
using System.Linq;
using System.Reflection;
using ReeperCommon.Logging;
using ScienceAlert.Annotations;
using ScienceAlert.Game;

namespace ScienceAlert.Commands
{
    public class DisablePluginCommand : ICommand
    {
        private readonly IAssemblyLoader _assemblyLoader;
        private readonly Assembly _target;
        private readonly ILog _log;

        public DisablePluginCommand([NotNull] IAssemblyLoader assemblyLoader, [NotNull] Assembly target, [NotNull] ILog log)
        {
            if (assemblyLoader == null) throw new ArgumentNullException("assemblyLoader");
            if (target == null) throw new ArgumentNullException("target");
            if (log == null) throw new ArgumentNullException("log");

            _assemblyLoader = assemblyLoader;
            _target = target;
            _log = log;
        }


        public void Execute()
        {
            var targetLoadedAssembly = _assemblyLoader.GetByAssembly(_target);

            if (!targetLoadedAssembly.Any())
            {
                _log.Error("Unable to find target " + _target.GetName().Name);
            }
            else
            {
                _assemblyLoader.Remove(targetLoadedAssembly.Single());
                _log.Warning("Plugin has been disabled.");
            }
        }
    }
}
