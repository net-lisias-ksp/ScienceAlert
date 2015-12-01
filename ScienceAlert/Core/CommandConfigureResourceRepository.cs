using System;
using System.IO;
using System.Linq;
using System.Reflection;
using ReeperCommon.FileSystem;
using ReeperCommon.Repositories;
using strange.extensions.command.impl;

namespace ScienceAlert.Core
{
// ReSharper disable once ClassNeverInstantiated.Global
    public class CommandConfigureResourceRepository : Command
    {
        private readonly IDirectory _pluginDirectory;
        private readonly Assembly _coreAssembly;

        public CommandConfigureResourceRepository(IDirectory pluginDirectory, Assembly coreAssembly)
        {
            if (pluginDirectory == null) throw new ArgumentNullException("pluginDirectory");
            if (coreAssembly == null) throw new ArgumentNullException("coreAssembly");
            _pluginDirectory = pluginDirectory;
            _coreAssembly = coreAssembly;
        }


        public override void Execute()
        {
            injectionBinder.Bind<IResourceRepository>().ToValue(ConfigureResourceRepository()).CrossContext();
        }

        // Removes extension from string, if an extension exists
        // Also converts windows-style \\ to / 
        private static string StripExtensionFromId(string id)
        {
            if (!Path.HasExtension(id) || string.IsNullOrEmpty(id)) return id;

            var dir = Path.GetDirectoryName(id) ?? "";
            var woExt = Path.Combine(dir, Path.GetFileNameWithoutExtension(id)).Replace('\\', '/');

            return !string.IsNullOrEmpty(woExt) ? woExt : id;
        }


        private static string ConvertUrlToResourceName(string url)
        {
            var prepend = 
#if DEBUG
                "ScienceAlert" // because AssemblyReloader mangles this
#else
                Assembly.GetExecutingAssembly().GetName().Name 
#endif 
                + ".";

            if (!url.StartsWith(prepend))
                url = prepend + url;

            return url.Replace('/', '.').Replace('\\', '.');
        }


        private IResourceRepository ConfigureResourceRepository()
        {
            var currentAssemblyResource = new ResourceFromEmbeddedResource(Assembly.GetExecutingAssembly());


            return new ResourceRepositoryComposite(
                // search GameDatabase first
                //   note: GameDatabase expects extensionless urls
                new ResourceIdentifierAdapter(StripExtensionFromId, new ResourceFromGameDatabase()),

                // then look at physical file system. These work on a list of items cached
                // by GameDatabase rather than working directly with the disk (unless a resource 
                // is accessed from here, of course)
                new ResourceFromDirectory(_pluginDirectory),

                // finally search embedded resource
                // we need to handle both cases of the url containing the extension or not...
                new ResourceRepositoryComposite(
                // don't strip extension
                    new ResourceIdentifierAdapter(ConvertUrlToResourceName, currentAssemblyResource),

                    // strip extension
                    new ResourceIdentifierAdapter(StripExtensionFromId,
                        new ResourceIdentifierAdapter(ConvertUrlToResourceName, currentAssemblyResource)),

                    // there's a potential third issue: if the incoming id doesn't contain an extension,
                // we might not fully match any of the manifest resource names. We'll add a special adapter
                // that will select the most-closely matching name if there's only one match
                    new ResourceIdentifierAdapter(ConvertUrlToResourceName,
                        new ResourceIdentifierAdapter(s =>
                        {
                            var resourceNames = _coreAssembly.GetManifestResourceNames()
                                .Where(name => name.StartsWith(s, StringComparison.InvariantCulture))
                                .ToList();

                            return resourceNames.Count == 1 ? resourceNames.First() : s;
                        }, currentAssemblyResource)
                        )));
        }
    }
}
