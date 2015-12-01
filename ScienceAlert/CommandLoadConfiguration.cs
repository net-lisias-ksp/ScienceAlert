using System;
using ReeperCommon.Logging;
using ReeperCommon.Serialization;
using ReeperCommon.Serialization.Exceptions;
using strange.extensions.command.impl;

namespace ScienceAlert
{
    public class CommandLoadConfiguration : Command
    {
        //private readonly ConfigNode _scenarioConfig;
        //private ScenarioConfiguration _configuration;
        //private readonly IConfigNodeSerializer _serializer;
        private readonly ILog _log;

        public CommandLoadConfiguration(
            //ConfigNode scenarioConfig, 
            //ScenarioConfiguration configuration, 
            //IConfigNodeSerializer serializer,
            ILog log)
        {
            //if (scenarioConfig == null) throw new ArgumentNullException("scenarioConfig");
            //if (configuration == null) throw new ArgumentNullException("configuration");
            //if (serializer == null) throw new ArgumentNullException("serializer");
            if (log == null) throw new ArgumentNullException("log");
            //_scenarioConfig = scenarioConfig;
            //_configuration = configuration;
            //_serializer = serializer;
            _log = log;
        }


        public override void Execute()
        {
            _log.Normal("LoadConfiguration command executed");

            //try
            //{
            //    _serializer.LoadObjectFromConfigNode(ref _configuration, _scenarioConfig);
            //    _log.Normal("Read from: {0}", _scenarioConfig.ToString());
            //}
            //catch (ReeperSerializationException rse)
            //{
            //    _log.Error("Failed to load configuration: " + rse);
            //}
        }
    }
}
