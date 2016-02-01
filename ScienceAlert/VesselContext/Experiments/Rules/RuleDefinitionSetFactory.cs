using System;
using System.Collections.Generic;
using System.Linq;
using ReeperCommon.Containers;

namespace ScienceAlert.VesselContext.Experiments.Rules
{
    public class RuleDefinitionSetFactory
    {
        private const string DefaultOnboardRuleNodeName = "SA_DEFAULT_ONBOARD_RULE";
        private const string DefaultAvailabilityRuleNodeName = "SA_DEFAULT_AVAILABILITY_RULE";
        private const string DefaultConditionRuleNodeName = "SA_DEFAULT_CONDITION_RULE";

        private const string OnboardRuleNodeName = "ONBOARD_RULE";
        private const string AvailabilityRuleNodeName = "AVAILABILITY_RULE";
        private const string ConditionRuleNodeName = "CONDITION_RULE";

        private readonly Lazy<ConfigNode> _defaultOnboardRuleConfig;
        private readonly Lazy<ConfigNode> _defaultAvailabilityRuleConfig;
        private readonly Lazy<ConfigNode> _defaultConditionRuleConfig;
 
        public RuleDefinitionSetFactory()
        {
            _defaultOnboardRuleConfig = new Lazy<ConfigNode>(() => GetConfigNodeFromDatabase(DefaultOnboardRuleNodeName));
            _defaultAvailabilityRuleConfig =
                new Lazy<ConfigNode>(() => GetConfigNodeFromDatabase(DefaultAvailabilityRuleNodeName));
            _defaultConditionRuleConfig =
                new Lazy<ConfigNode>(() => GetConfigNodeFromDatabase(DefaultConditionRuleNodeName));
        }


        private ConfigNode GetConfigNodeFromDatabase(string nodeName)
        {
            if (string.IsNullOrEmpty(nodeName))
                throw new ArgumentException("Must supply a non-empty name", "nodeName");

            var matches = GameDatabase.Instance.GetConfigNodes(nodeName);

            var found = GetSingleNodeByName(matches, nodeName);

            if (!found.Any())
                throw new ConfigNodeNotFoundException(nodeName);

            return found.Value;
        }


        private static Maybe<ConfigNode> GetSingleNodeByName(IEnumerable<ConfigNode> configNodes, string name)
        {
            var matches = configNodes.Where(c => c.name == name).ToList();

            if (!matches.Any())
                return Maybe<ConfigNode>.None;

            if (matches.Count > 1)
                throw new ArgumentException("Multiple ConfigNodes of name " + name + " found", "nodeName");

            return matches.Single().ToMaybe();
        }


        private static Maybe<ConfigNode> GetSingleNodeByName(ConfigNode config, string name)
        {
            var matchingNodes = config.GetNodes(name);
            return GetSingleNodeByName(matchingNodes, name);
        }
        

        // note that the ConfigNode is _optional_ -- there will be many cases (especially for stock experiments) where no special
        // rules are defined for the experiment and defaults are fine
        public RuleDefinitionSet Create(ScienceExperiment experiment, Maybe<ConfigNode> config)
        {
            if (experiment == null) throw new ArgumentNullException("experiment");


            return new RuleDefinitionSet(experiment,
                config.With(c => GetSingleNodeByName(c.Value, OnboardRuleNodeName))
                    .Or(_defaultOnboardRuleConfig.Value),
                config.With(c => GetSingleNodeByName(c.Value, AvailabilityRuleNodeName))
                    .Or(_defaultAvailabilityRuleConfig.Value),
                config.With(c => GetSingleNodeByName(c.Value, ConditionRuleNodeName))
                    .Or(_defaultConditionRuleConfig.Value));
 
        }
    }
}
