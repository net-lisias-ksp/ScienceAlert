using System;
using System.IO;
using ReeperCommon.FileSystem;
using strange.extensions.implicitBind;
using strange.extensions.injector.api;

namespace ScienceAlert.Gui
{
    [Implements(typeof(IGetGuiConfigurationFilePath), InjectionBindingScope.CROSS_CONTEXT)]
// ReSharper disable once UnusedMember.Global
    public class GetGuiConfigurationFilePath : IGetGuiConfigurationFilePath
    {
        private const string GuiConfigurationFileName = "settings.dat";

        private readonly IDirectory _pluginDirectory;

        public GetGuiConfigurationFilePath(IDirectory pluginDirectory)
        {
            if (pluginDirectory == null) throw new ArgumentNullException("pluginDirectory");
            _pluginDirectory = pluginDirectory;
        }


        public string Get()
        {
            return Path.Combine(_pluginDirectory.FullPath, GuiConfigurationFileName);
        }
    }
}
