using System;
using System.Collections.Generic;
using System.Linq;
using ReeperCommon.Containers;
using ReeperCommon.Extensions;
using ScienceAlert.Game;
using ScienceAlert.VesselContext.Experiments.Rules;

namespace ScienceAlert.VesselContext.Experiments
{
    public class ExperimentSensorFactory
    {
        private const string DefaultOnboardRuleNodeName = "SA_DEFAULT_ONBOARD_RULE";
        private const string DefaultAvailabilityRuleNodeName = "SA_DEFAULT_AVAILABILITY_RULE";
        private const string DefaultConditionRuleNodeName = "SA_DEFAULT_CONDITION_RULE";

        private const string OnboardRuleNodeName = "ONBOARD_RULE";
        private const string AvailabilityRuleNodeName = "AVAILABILITY_RULE";
        private const string ConditionRuleNodeName = "CONDITION_RULE";

        private const string ExperimentRuleConfigNodeName = "SA_EXPERIMENT_RULESET";
        private const string RuleExperimentIdValueName = "experimentID";

        private readonly IScienceSubjectProvider _subjectProvider;
        private readonly IExperimentReportValueCalculator _reportValueCalculator;
        private readonly IExperimentRuleFactory _ruleFactory;

        private readonly Lazy<ConfigNode> _defaultOnboardRuleConfig;
        private readonly Lazy<ConfigNode> _defaultAvailabilityRuleConfig; 
        private readonly Lazy<ConfigNode> _defaultConditionRuleConfig;
         
        private readonly Lazy<Dictionary<string, ConfigNode>> _customRulesets;
  

        public ExperimentSensorFactory(
            IScienceSubjectProvider subjectProvider,
            IExperimentReportValueCalculator reportValueCalculator,
            IExperimentRuleFactory ruleFactory
            )
        {
            if (subjectProvider == null) throw new ArgumentNullException("subjectProvider");
            if (reportValueCalculator == null) throw new ArgumentNullException("reportValueCalculator");
            if (ruleFactory == null) throw new ArgumentNullException("ruleFactory");

            _subjectProvider = subjectProvider;
            _reportValueCalculator = reportValueCalculator;
            _ruleFactory = ruleFactory;

            _defaultOnboardRuleConfig = new Lazy<ConfigNode>(() => GetDefaultConfig(DefaultOnboardRuleNodeName));
            _defaultAvailabilityRuleConfig =
                new Lazy<ConfigNode>(() => GetDefaultConfig(DefaultAvailabilityRuleNodeName));
            _defaultConditionRuleConfig = new Lazy<ConfigNode>(() => GetDefaultConfig(DefaultConditionRuleNodeName));

            _customRulesets = new Lazy<Dictionary<string, ConfigNode>>(BuildCustomRuleConfigDictionary);
        }


        private static ConfigNode GetDefaultConfig(string nodeName)
        {
            if (string.IsNullOrEmpty(nodeName))
                throw new ArgumentException("Must provide a valid node name", "nodeName");

            var possibilities = GameDatabase.Instance.GetConfigs(nodeName);

            if (possibilities.Length > 1)
            {
                Log.Error("Multiple nodes match " + nodeName);
                possibilities.ToList().ForEach(urlConfig => Log.Error(nodeName + " at " + urlConfig.url));

                throw new ArgumentException("Multiple ConfigNodes match " + nodeName);
            }
            if (possibilities.Length == 0)
                throw new ArgumentException("No ConfigNodes match " + nodeName);

            return possibilities.Single().config;
        }


        private static Dictionary<string, ConfigNode> BuildCustomRuleConfigDictionary()
        {
            var experimentRulesetConfigs = GameDatabase.Instance.GetConfigs(ExperimentRuleConfigNodeName)
                .Where(urlConfig => urlConfig.config.GetValueEx(RuleExperimentIdValueName).Any())
                .Select(
                    urlConfig =>
                        new KeyValuePair<string, UrlDir.UrlConfig>(urlConfig.config.GetValueEx(RuleExperimentIdValueName).Value, urlConfig))
                .ToList();

            var duplicates = experimentRulesetConfigs.GroupBy(kvp => kvp.Key)
                .Where(grouping => grouping.Count() > 1)
                .ToList();

            var dictionary = new Dictionary<string, ConfigNode>();

            if (duplicates.Any())
            {
                Log.Warning("Found duplicate experiment rulesets!");

                foreach (var grouping in duplicates)
                {
                    var experimentId = grouping.Key;

                    foreach (var item in grouping.Select(k => k))
                        Log.Warning(experimentId + " at location " + item.Value.url);
                }
            }

            foreach (var config in experimentRulesetConfigs.Where(config => !dictionary.ContainsKey(config.Key)))
            {
                dictionary.Add(config.Key, config.Value.config);
                Log.Debug("Loaded custom experiment ruleset for " + config.Key + " from " + config.Value.url);
            }

            return dictionary;
        }


        private RuleDefinitionSet GetRulesetFor(ScienceExperiment experiment)
        {
            if (experiment == null) throw new ArgumentNullException("experiment");

            ConfigNode customRule;

            if (!_customRulesets.Value.TryGetValue(experiment.id, out customRule))
                customRule = new ConfigNode(ExperimentRuleConfigNodeName);

            // the ruleset might not contain data for every rule type, so if it's missing any
            // we'll insert the default rule 

            return new RuleDefinitionSet(
                customRule.Return(c => c.GetNode(OnboardRuleNodeName), _defaultOnboardRuleConfig.Value),
                customRule.Return(c => c.GetNode(AvailabilityRuleNodeName), _defaultAvailabilityRuleConfig.Value),
                customRule.Return(c => c.GetNode(ConditionRuleNodeName), _defaultConditionRuleConfig.Value));
        }


        public ExperimentSensor Create(ScienceExperiment experiment)
        {
            if (experiment == null) throw new ArgumentNullException("experiment");

            var ruleset = GetRulesetFor(experiment);

            return new ExperimentSensor(experiment, _subjectProvider, _reportValueCalculator,
                _ruleFactory.Create(experiment, ruleset.OnboardRuleDefinition),
                _ruleFactory.Create(experiment, ruleset.AvailabilityRuleDefinition),
                _ruleFactory.Create(experiment, ruleset.ConditionRuleDefinition));
        }
    }
}
