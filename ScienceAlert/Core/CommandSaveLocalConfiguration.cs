using System;
using JetBrains.Annotations;
using ReeperCommon.Logging;
using ReeperKSP.Extensions;
using ReeperKSP.Serialization;
using strange.extensions.command.impl;

namespace ScienceAlert.Core
{
    // ReSharper disable once ClassNeverInstantiated.Global
    class CommandSaveLocalConfiguration : Command
    {
        private readonly ConfigNode _scenarioConfig;
        private readonly LocalConfiguration _localConfiguration;
        private readonly IConfigNodeSerializer _serializer;

        public CommandSaveLocalConfiguration(
            [NotNull] ConfigNode scenarioConfig,
            [NotNull] LocalConfiguration localConfiguration, 
            [NotNull] IConfigNodeSerializer serializer)
        {
            if (scenarioConfig == null) throw new ArgumentNullException("scenarioConfig");
            if (localConfiguration == null) throw new ArgumentNullException("localConfiguration");
            if (serializer == null) throw new ArgumentNullException("serializer");
            _scenarioConfig = scenarioConfig;
            _localConfiguration = localConfiguration;
            _serializer = serializer;
        }

        public override void Execute()
        {
            Log.Verbose("Saving local configuration");
            _scenarioConfig.Set("temporaryValue", "someValue");

            var lc = _localConfiguration;
            _serializer.LoadObjectFromConfigNode(ref lc, _scenarioConfig);

            var serialized = _serializer.CreateConfigNodeFromObject(_localConfiguration);

            Log.Warning("Serialized local configuration: " + serialized.ToSafeString());
        }
    }
}
