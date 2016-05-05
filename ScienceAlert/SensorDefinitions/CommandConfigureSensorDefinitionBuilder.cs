using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using ReeperCommon.Extensions;
using ReeperCommon.Logging;
using ReeperKSP.Extensions;
using ScienceAlert.Core;
using ScienceAlert.Game;
using strange.extensions.command.impl;

namespace ScienceAlert.SensorDefinitions
{
// ReSharper disable once ClassNeverInstantiated.Global
    class CommandConfigureSensorDefinitionBuilder : Command
    {
        private const string DefaultOnboardRuleNodeName = "SA_DEFAULT_ONBOARD_RULE";
        private const string DefaultAvailabilityRuleNodeName = "SA_DEFAULT_AVAILABILITY_RULE";
        private const string DefaultConditionRuleNodeName = "SA_DEFAULT_CONDITION_RULE";
        private const string DefaultExperimentTriggerNodeName = "SA_DEFAULT_EXPERIMENT_TRIGGER";

        private readonly ReadOnlyCollection<ScienceExperiment> _experiments;
        private readonly IGameDatabase _gameDatabase;

        public CommandConfigureSensorDefinitionBuilder(ReadOnlyCollection<ScienceExperiment> experiments, IGameDatabase gameDatabase)
        {
            if (experiments == null) throw new ArgumentNullException("experiments");
            if (gameDatabase == null) throw new ArgumentNullException("gameDatabase");

            _experiments = experiments;
            _gameDatabase = gameDatabase;
        }


        private ConfigNode GetSingleConfigNode(string nodeName)
        {
            var configs = _gameDatabase.GetConfigs(nodeName);

            if (!configs.Any())
                throw new ArgumentException("GameDatabase does not contain any node named " + nodeName, "nodeName");

            if (configs.Length == 1) return configs.Single().Config;

            foreach (var urlC in configs)
                Log.Error("Duplicate ConfigNode '" + nodeName + "': " + urlC.Url);
            throw new ArgumentException("GameDatabase contains multiple entries named " + nodeName, "nodeName");
        }


        private ConfigNode GetSingleSubConfigNode(ConfigNode parent)
        {
            if (parent == null) throw new ArgumentNullException("parent");

            if (parent.CountNodes != 1)
                throw new ArgumentException("The ConfigNode " + parent.ToSafeString() +
                                            " must contain exactly one subnode");

            return parent.nodes[0];
        }


        public override void Execute()
        {
            var defaultOnboardRuleConfig = GetSingleSubConfigNode(GetSingleConfigNode(DefaultOnboardRuleNodeName));
            var defaultAvailabilityRuleConfig = GetSingleSubConfigNode(GetSingleConfigNode(DefaultAvailabilityRuleNodeName));
            var defaultConditionRuleConfig = GetSingleSubConfigNode(GetSingleConfigNode(DefaultConditionRuleNodeName));
            var defaultTriggerConfig = GetSingleSubConfigNode(GetSingleConfigNode(DefaultExperimentTriggerNodeName));

            var builder = new SensorDefinitionBuilder(_experiments, defaultOnboardRuleConfig,
                defaultAvailabilityRuleConfig, defaultConditionRuleConfig, defaultTriggerConfig);

            injectionBinder
                .Bind<IConfigNodeObjectBuilder<SensorDefinition>>()
                .Bind<ISensorDefinitionFactory>()
                .To(builder);
        }
    }
}
