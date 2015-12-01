using System;
using ReeperCommon.Containers;
using ReeperCommon.Logging;
using ReeperCommon.Serialization;
using ReeperCommon.Serialization.Exceptions;
using strange.extensions.command.impl;

namespace ScienceAlert
{
    public class CommandSaveConfiguration : Command
    {
        //private readonly ConfigNode _scenarioNode;
        //private readonly ScenarioConfiguration _scenarioConfiguration;
        //private readonly IConfigNodeSerializer _serializer;
        private readonly ILog _log;

        public CommandSaveConfiguration(
            //ConfigNode scenarioNode, 
            //ScenarioConfiguration scenarioConfiguration, 
            //IConfigNodeSerializer serializer, 
            ILog log)
        {
            //if (scenarioNode == null) throw new ArgumentNullException("scenarioNode");
            //if (scenarioConfiguration == null) throw new ArgumentNullException("ScenarioConfiguration");
            //if (serializer == null) throw new ArgumentNullException("serializer");
            if (log == null) throw new ArgumentNullException("log");

            //_scenarioNode = scenarioNode;
            //_scenarioConfiguration = scenarioConfiguration;
            //_serializer = serializer;
            _log = log;
        }


        public override void Execute()
        {
            _log.Normal("SaveConfiguration Command executed");
            //_log.Normal("Existing: {0}", _scenarioNode.ToString());

            //try
            //{
            //    var saved = _serializer.CreateConfigNodeFromObject(_scenarioConfiguration);

            //    _scenarioNode.GetNode(saved.name)
            //        .Do(n => _scenarioNode.RemoveNode(saved.name))
            //        .IfNull(() => _scenarioNode.AddNode(saved));
            //}
            //catch (ReeperSerializationException rse)
            //{
            //    _log.Error("Exception while saving configuration: " + rse);
            //}
        }
    }
}
