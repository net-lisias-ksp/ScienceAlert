using System;
using ReeperCommon.Extensions;
using ReeperCommon.Logging;
using ReeperCommon.Serialization;
using ReeperCommon.Serialization.Exceptions;
using strange.extensions.command.impl;
using strange.extensions.injector;

namespace ScienceAlert.Gui
{
// ReSharper disable once ClassNeverInstantiated.Global
    public class CommandSaveGuiConfiguration : Command
    {
        private readonly IGetGuiConfigurationFilePath _configFileQuery;
        private readonly string _nodeName;
        private GuiConfiguration _guiConfiguration;
        private readonly IConfigNodeSerializer _serializer;
        private readonly SignalSaveGuiSettings _saveSettings;
        private readonly ILog _log;

        public CommandSaveGuiConfiguration(
            IGetGuiConfigurationFilePath configFileQuery, 
            [Name(Keys.GuiSettingsNodeName)] string nodeName,
            GuiConfiguration guiConfiguration,
            IConfigNodeSerializer serializer,
            SignalSaveGuiSettings saveSettings,
            ILog log)
        {
            if (configFileQuery == null) throw new ArgumentNullException("configFileQuery");
            if (nodeName == null) throw new ArgumentNullException("nodeName");
            if (guiConfiguration == null) throw new ArgumentNullException("guiConfiguration");
            if (serializer == null) throw new ArgumentNullException("serializer");
            if (saveSettings == null) throw new ArgumentNullException("saveSettings");
            if (log == null) throw new ArgumentNullException("log");

            _configFileQuery = configFileQuery;
            _nodeName = nodeName;
            _guiConfiguration = guiConfiguration;
            _serializer = serializer;
            _saveSettings = saveSettings;
            _log = log;
        }

        public override void Execute()
        {
            try
            {
                var configPath = _configFileQuery.Get();
                var node = new ConfigNode(_nodeName, "Science Alert Shared GUI settings");

                SaveNonWindowSpecificSettings(node);

                _log.Normal("Saving GUI configuration to " + configPath);
                _saveSettings.Dispatch(node);

                if (!node.HasData)
                    _log.Error("No data saved. Something went wrong");
                else node.Write(configPath, "ScienceAlert Window Configuration");
            }
            catch (Exception e)
            {
                _log.Error("Error while saving GUI configuration: " + e);
                _log.Warning("GUI configuration was NOT saved");
            }
        }


        private void SaveNonWindowSpecificSettings(ConfigNode config)
        {
            if (config == null) throw new ArgumentNullException("config");

            _serializer.WriteObjectToConfigNode(ref _guiConfiguration, config);
        }
    }
}
