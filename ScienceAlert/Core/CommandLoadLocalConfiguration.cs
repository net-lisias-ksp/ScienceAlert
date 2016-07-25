using System;
using System.Linq;
using JetBrains.Annotations;
using ReeperCommon.Logging;
using ReeperKSP.Extensions;
using ReeperKSP.Serialization;
using ReeperKSP.Serialization.Exceptions;
using strange.extensions.command.impl;

namespace ScienceAlert.Core
{
    // ReSharper disable once ClassNeverInstantiated.Global
    class CommandLoadLocalConfiguration : Command
    {
        private readonly ConfigNode _scenarioConfig;
        private readonly LocalConfiguration _localConfig;
        private readonly IConfigNodeSerializer _serializer;

        public CommandLoadLocalConfiguration(
            [NotNull] ConfigNode scenarioConfig, 
            [NotNull] LocalConfiguration localConfig,
            [NotNull] IConfigNodeSerializer serializer)
        {
            if (scenarioConfig == null) throw new ArgumentNullException("scenarioConfig");
            if (localConfig == null) throw new ArgumentNullException("localConfig");
            if (serializer == null) throw new ArgumentNullException("serializer");

            _scenarioConfig = scenarioConfig;
            _localConfig = localConfig;
            _serializer = serializer;
        }


        public override void Execute()
        {
            Log.Verbose("Loading local configuration...");

            var localConfig = _scenarioConfig.GetNodeEx(LocalConfiguration.LocalConfigurationScenarioModuleNodeName);

            if (!localConfig.Any())
            {
                Log.Warning("No local configuration found; defaults will be used");
                _localConfig.GenerateDefaultSettingsForMissingExperiments();
            }
            else
            {
                try
                {
                    var lc = _localConfig;
                    _serializer.LoadObjectFromConfigNode(ref lc, localConfig.Value);
                    Log.Normal("Loaded local configuration");
                }
                catch (ReeperSerializationException rse)
                {
                    Log.Error("Error while deserializing local configuration: " + rse); // todo: revert to default state?
                }
            }
        }
    }
}
