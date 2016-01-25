//using System;
//using ReeperCommon.Containers;
//using ReeperCommon.Extensions;
//using ReeperCommon.Logging;
//using ReeperCommon.Serialization;
//using strange.extensions.command.impl;
//using strange.extensions.injector;
//using File = System.IO.File;

//namespace ScienceAlert.Gui
//{
//// ReSharper disable once ClassNeverInstantiated.Global
//    public class CommandLoadGuiConfiguration : Command
//    {
//        private readonly SignalLoadGuiSettings _loadSignal;
//        private readonly ISharedConfigurationFilePathProvider _configPathQuery;
//        private readonly string _nodeName;
//        private SharedConfiguration _guiConfiguration;
//        private readonly IConfigNodeSerializer _serializer;
//        private readonly ILog _log;

//        public CommandLoadGuiConfiguration(
//            SignalLoadGuiSettings loadSignal,
//            ISharedConfigurationFilePathProvider configPathQuery, 
//            [Name(CoreKeys.GuiSettingsNodeName)] string nodeName,
//            SharedConfiguration guiConfiguration,
//            IConfigNodeSerializer serializer,
//            ILog log)
//        {
//            if (loadSignal == null) throw new ArgumentNullException("loadSignal");
//            if (configPathQuery == null) throw new ArgumentNullException("configPathQuery");
//            if (nodeName == null) throw new ArgumentNullException("nodeName");
//            if (guiConfiguration == null) throw new ArgumentNullException("SharedConfiguration");
//            if (serializer == null) throw new ArgumentNullException("serializer");
//            if (log == null) throw new ArgumentNullException("log");

//            _loadSignal = loadSignal;
//            _configPathQuery = configPathQuery;
//            _nodeName = nodeName;
//            _guiConfiguration = guiConfiguration;
//            _serializer = serializer;
//            _log = log;
//        }


//        public override void Execute()
//        {
//            try
//            {
//                var configPath = _configPathQuery.Passes();

//                if (!File.Exists(configPath))
//                {
//                    _log.Warning("Could not find GUI configuration at " + configPath + "; defaults will be used");
//                    return;
//                }

//                _log.Normal("Loading GUI configuration from " + configPath);

//                ConfigNode.Load(configPath)
//                    .IfNull(() => { throw new InvalidConfigNodeException(configPath); })
//                    .With(n => n.GetNode(_nodeName))
//                    .IfNull(() => { throw new ConfigNodeNotFoundException(_nodeName); })
//                    .Do(n => { if (!n.HasData) throw new EmptyConfigNodeException(); })
//                    .Do(LoadNonWindowSpecificSettings)
//                    .Do(n => _loadSignal.Dispatch(n));

//                _log.Normal("GUI configuration successfully loaded");
//            }
//            catch (Exception e)
//            {
//                _log.Error("Error while loading GUI configuration: " + e);
//                _log.Warning("GUI configuration was NOT loaded; defaults will be used");
//            }
//        }


//        private void LoadNonWindowSpecificSettings(ConfigNode node)
//        {
//            if (node == null) throw new ArgumentNullException("node");

//            _serializer.LoadObjectFromConfigNode(ref _guiConfiguration, node);
//        }
//    }
//}
