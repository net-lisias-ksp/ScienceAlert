using System;
using System.Collections.Generic;
using System.Linq;
using ReeperCommon.Containers;
using ReeperCommon.Extensions;
using ScienceAlert.Core;

namespace ScienceAlert.SensorDefinitions
{
    // Creates a SensorDefinition given a ConfigNode that matches "SA_EXPERIMENT_RULESET"
    public class SensorDefinitionBuilder : IConfigNodeObjectBuilder<SensorDefinition>, ISensorDefinitionFactory
    {
        private readonly ConfigNode _defaultOnboardRule;
        private readonly ConfigNode _defaultAvailabilityRule;
        private readonly ConfigNode _defaultConditionRule;

        private readonly ScienceExperiment[] _experiments;


        public const string ExperimentRulesetNodeName = "SA_EXPERIMENT_RULESET";
        public const string ExperimentIdValueName = "experimentID";

        public const string OnboardRuleNodeName = "ONBOARD_RULE";
        public const string AvailabilityRuleNodeName = "AVAILABILITY_RULE";
        public const string ConditionRuleNodeName = "CONDITION_RULE";


        public SensorDefinitionBuilder(
            IEnumerable<ScienceExperiment> experiments,
            ConfigNode defaultOnboardRule,
            ConfigNode defaultAvailabilityRule,
            ConfigNode defaultConditionRule)
        {
            if (experiments == null) throw new ArgumentNullException("experiments");
            if (defaultOnboardRule == null) throw new ArgumentNullException("defaultOnboardRule");
            if (defaultAvailabilityRule == null)
                throw new ArgumentNullException("defaultAvailabilityRule");
            if (defaultConditionRule == null) throw new ArgumentNullException("defaultConditionRule");

            _defaultOnboardRule = defaultOnboardRule;
            _defaultAvailabilityRule = defaultAvailabilityRule;
            _defaultConditionRule = defaultConditionRule;
            _experiments = experiments.ToArray();
        }


        private static Maybe<ConfigNode> GetConfig(ConfigNode config, string nodeName)
        {
            return config.GetNodeEx(nodeName, false)
                .Do(cn =>
                {
                    if (cn.CountNodes != 1)
                        throw new ArgumentException("ConfigNode '" + nodeName + "' must contain exactly one subnode",
                            "config");
                }).With(cn => cn.nodes[0]);
        }


        private Maybe<ScienceExperiment> GetExperiment(string experimentId)
        {
            if (string.IsNullOrEmpty(experimentId))
                throw new ArgumentException("must specify experiment id", "experimentId");

            return _experiments.FirstOrDefault(se => se.id == experimentId).ToMaybe();
        }


        private static Maybe<string> GetExperimentId(ConfigNode config)
        {
            return config.GetValueEx(ExperimentIdValueName);
        }


        public SensorDefinition Build(ConfigNode config)
        {
            if (!CanHandle(config))
                throw new ArgumentException("This builder can't handle " + config);

            var experimentId = GetExperimentId(config);

            if (!experimentId.Any())
                throw new ArgumentException("config does not contain an " + ExperimentIdValueName + " entry");

            var experiment = GetExperiment(experimentId.Value);

            if (!experiment.Any())
                throw new ArgumentException(experimentId.Value + " is unrecognized");

            return new SensorDefinition(
                experiment.Value,
                GetConfig(config, OnboardRuleNodeName).Or(_defaultOnboardRule).CreateCopy(), // note: copies used to guarantee that factories building from them don't make unintended changes
                GetConfig(config, AvailabilityRuleNodeName).Or(_defaultAvailabilityRule).CreateCopy(),
                GetConfig(config, ConditionRuleNodeName).Or(_defaultConditionRule).CreateCopy());
        }


        public SensorDefinition CreateDefault(ScienceExperiment experiment)
        {
            return new SensorDefinition(experiment, _defaultOnboardRule, _defaultAvailabilityRule,
                _defaultAvailabilityRule);
        }


        public bool CanHandle(ConfigNode config)
        {
            if (config == null) throw new ArgumentNullException("config");

            return config.name == ExperimentRulesetNodeName &&
                GetExperimentId(config)
                    .With(GetExperiment)
                    .Any();
        }
    }
}
