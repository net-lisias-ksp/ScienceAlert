using System;
using JetBrains.Annotations;
using ReeperCommon.Logging;
using strange.extensions.command.impl;

namespace ScienceAlert.Core
{
    // ReSharper disable once ClassNeverInstantiated.Global
    class CommandLoadLocalConfiguration : Command
    {
        private readonly ConfigNode _scenarioConfig;
        private readonly LocalConfiguration _localConfig;

        public CommandLoadLocalConfiguration([NotNull] ConfigNode scenarioConfig, [NotNull] LocalConfiguration localConfig)
        {
            if (scenarioConfig == null) throw new ArgumentNullException("scenarioConfig");
            if (localConfig == null) throw new ArgumentNullException("localConfig");
            _scenarioConfig = scenarioConfig;
            _localConfig = localConfig;
        }

        public override void Execute()
        {
            Log.Warning("Local configuration would be loaded at this point");
        }
    }
}
