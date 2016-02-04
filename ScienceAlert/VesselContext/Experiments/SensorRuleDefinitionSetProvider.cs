using System;
using System.Collections.Generic;
using System.Linq;
using ReeperCommon.Containers;
using ReeperCommon.Extensions;
using ScienceAlert.Game;

namespace ScienceAlert.VesselContext.Experiments
{
    public class SensorRuleDefinitionSetProvider : ISensorRuleDefinitionSetProvider
    {
        public const string DefaultOnboardRuleNodeName = "SA_DEFAULT_ONBOARD_RULE";
        public const string DefaultAvailabilityRuleNodeName = "SA_DEFAULT_AVAILABILITY_RULE";
        public const string DefaultConditionRuleNodeName = "SA_DEFAULT_CONDITION_RULE";

        public const string OnboardRuleNodeName = "ONBOARD_RULE";
        public const string AvailabilityRuleNodeName = "AVAILABILITY_RULE";
        public const string ConditionRuleNodeName = "CONDITION_RULE";

        public const string ExperimentRuleConfigNodeName = "SA_EXPERIMENT_RULESET";
        public const string RuleExperimentIdValueName = "experimentID";

        private readonly Lazy<ConfigNode> _defaultOnboardRuleConfig;
        private readonly Lazy<ConfigNode> _defaultAvailabilityRuleConfig;
        private readonly Lazy<ConfigNode> _defaultConditionRuleConfig;
        private readonly Lazy<Dictionary<string, SensorRuleDefinitionSet>> _rulePackages;
        private readonly Lazy<SensorRuleDefinitionSet> _defaultRuleDefinitionSet;
 
        private readonly IGameDatabase _gameDatabase;

        public SensorRuleDefinitionSetProvider(IGameDatabase gameDatabase)
        {
            if (gameDatabase == null) throw new ArgumentNullException("gameDatabase");

            _gameDatabase = gameDatabase;

            _defaultOnboardRuleConfig = new Lazy<ConfigNode>(() =>
            {
                var onboardConfig = GetSingleConfigFromDatabase(DefaultOnboardRuleNodeName).CreateCopy();
                onboardConfig.name = OnboardRuleNodeName;

                return onboardConfig;
            });

            _defaultAvailabilityRuleConfig =
                new Lazy<ConfigNode>(() =>
                {
                    var availabilityConfig = GetSingleConfigFromDatabase(DefaultAvailabilityRuleNodeName).CreateCopy();
                    availabilityConfig.name = AvailabilityRuleNodeName;
                    return availabilityConfig;
                });

            _defaultConditionRuleConfig = new Lazy<ConfigNode>(() =>
            {
                var conditionConfig = GetSingleConfigFromDatabase(DefaultConditionRuleNodeName).CreateCopy();
                conditionConfig.name = ConditionRuleNodeName;
                return conditionConfig;
            });

            _rulePackages = new Lazy<Dictionary<string, SensorRuleDefinitionSet>>(BuildCustomRulePackageDictionary);
            _defaultRuleDefinitionSet = new Lazy<SensorRuleDefinitionSet>(() => CreateRulesetFor(Maybe<ConfigNode>.None));
        }


        private ConfigNode GetSingleConfigFromDatabase(string nodeName)
        {
            if (string.IsNullOrEmpty(nodeName))
                throw new ArgumentException("Must provide a valid node name", "nodeName");

            var possibilities = _gameDatabase.GetConfigs(nodeName);

            if (possibilities.Length > 1)
            {
                Log.Error("Multiple nodes match " + nodeName);
                possibilities.ToList().ForEach(urlConfig => Log.Error(nodeName + " at " + urlConfig.Url));

                throw new ArgumentException("Multiple ConfigNodes match " + nodeName);
            }
            if (possibilities.Length == 0)
                throw new ArgumentException("No ConfigNodes match " + nodeName);

            //var ruleConfig = possibilities.Single().config;


            //// if this config contains more than one node, it's most likely that the config author meant
            //// all of the rules that follow should pass. We'll make a bit of noise about it though
            //if (ruleConfig.CountNodes > 1)
            //{
            //    if (ruleConfig.CountValues > 0)
            //        Log.Warning("ConfigNode " + nodeName + " contains values which will be ignored");

            //    Log.Verbose("Default node entry " + nodeName + " contains multiple rule entries; assuming all must pass");
            //    var combined = new ConfigNode(CompositeRule.CompositeType.All.ToString());

            //    for (int i = 0; i < ruleConfig.CountNodes; ++i)
            //        combined.AddNode(ruleConfig.nodes[i]);

            //    return combined;
            //}

            return possibilities.Single().Config;
        }



        private Dictionary<string, SensorRuleDefinitionSet> BuildCustomRulePackageDictionary()
        {
            var raw = _gameDatabase.GetConfigs(ExperimentRuleConfigNodeName);
            var raw2 = GameDatabase.Instance.GetConfigNodes(ExperimentRuleConfigNodeName);

            var experimentRulesetConfigs = _gameDatabase.GetConfigs(ExperimentRuleConfigNodeName)
                .Where(urlConfig => urlConfig.Config.GetValueEx(RuleExperimentIdValueName).Any())
                .Select(
                    urlConfig =>
                        new KeyValuePair<string, IUrlConfig>(urlConfig.Config.GetValueEx(RuleExperimentIdValueName).Value, urlConfig))
                .ToList();

            var duplicates = experimentRulesetConfigs.GroupBy(kvp => kvp.Key)
                .Where(grouping => grouping.Count() > 1)
                .ToList();

            var dictionary = new Dictionary<string, SensorRuleDefinitionSet>();

            if (duplicates.Any())
            {
                Log.Warning("Found duplicate experiment rulesets!");

                foreach (var grouping in duplicates)
                {
                    var experimentId = grouping.Key;

                    foreach (var item in grouping.Select(k => k))
                        Log.Warning(experimentId + " at location " + item.Value.Url);
                }
            }

            foreach (var config in experimentRulesetConfigs.Where(config => !dictionary.ContainsKey(config.Key)))
            {
                dictionary.Add(config.Key, CreateRulesetFor(config.Value.Config.ToMaybe()));
                Log.Debug("Loaded custom experiment ruleset for " + config.Key + " from " + config.Value.Url);
            }

            return dictionary;
        }


        public SensorRuleDefinitionSet CreateRulesetFor(Maybe<ConfigNode> customRule)
        {
            if (!customRule.Any())
                customRule = new ConfigNode(ExperimentRuleConfigNodeName).ToMaybe();

            // the ruleset might not contain data for every rule type, so if it's missing any
            // we'll insert the default rule 

            return new SensorRuleDefinitionSet(
                customRule.With(c => c.GetNode(OnboardRuleNodeName)).Or(_defaultOnboardRuleConfig.Value),
                customRule.With(c => c.GetNode(AvailabilityRuleNodeName)).Or(_defaultAvailabilityRuleConfig.Value),
                customRule.With(c => c.GetNode(ConditionRuleNodeName)).Or(_defaultConditionRuleConfig.Value));
        }



        public Maybe<SensorRuleDefinitionSet> GetDefinitionSet(ScienceExperiment experiment)
        {
            if (experiment == null) throw new ArgumentNullException("experiment");

            return !_rulePackages.Value.ContainsKey(experiment.id) ? Maybe<SensorRuleDefinitionSet>.None : _rulePackages.Value[experiment.id].ToMaybe();
        }


        public SensorRuleDefinitionSet GetDefaultDefinition()
        {
            return _defaultRuleDefinitionSet.Value;
        }
    }
}
