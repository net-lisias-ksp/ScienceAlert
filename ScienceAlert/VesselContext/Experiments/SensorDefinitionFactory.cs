using System;
using System.Collections.Generic;
using System.Linq;
using ReeperCommon.Containers;
using ReeperCommon.Extensions;
using ScienceAlert.Game;
using ScienceAlert.VesselContext.Experiments.Rules;

namespace ScienceAlert.VesselContext.Experiments
{
    // Creates a SensorDefinition given a ConfigNode that matches "SA_EXPERIMENT_RULESET"
    public class SensorDefinitionFactory : IConfigNodeObjectGraphBuilder<SensorDefinition>, ISensorDefinitionFactory
    {
        private readonly IRuleFactory _defaultOnboardRuleFactory;
        private readonly IRuleFactory _defaultAvailabilityRuleFactory;
        private readonly IRuleFactory _defaultConditionRuleFactory;
        private readonly IConfigNodeObjectGraphBuilder<IRuleFactory> _ruleFactoryBuilder;

        private readonly ScienceExperiment[] _experiments;

        public const string ExperimentRulesetNodeName = "SA_EXPERIMENT_RULESET";
        public const string ExperimentIdValueName = "experimentID";

        public const string OnboardRuleNodeName = "ONBOARD_RULE";
        public const string AvailabilityRuleNodeName = "AVAILABILITY_RULE";
        public const string ConditionRuleNodeName = "CONDITION_RULE";

        private SensorDefinitionFactory(
            IEnumerable<ScienceExperiment> experiments,
            IRuleFactory defaultOnboardRuleFactory,
            IRuleFactory defaultAvailabilityRuleFactory,
            IRuleFactory defaultConditionRuleFactory,
            IConfigNodeObjectGraphBuilder<IRuleFactory> ruleFactoryBuilder)
        {
            if (ruleFactoryBuilder == null) throw new ArgumentNullException("ruleFactoryBuilder");
            if (experiments == null) throw new ArgumentNullException("experiments");
            if (defaultOnboardRuleFactory == null) throw new ArgumentNullException("defaultOnboardRuleFactory");
            if (defaultAvailabilityRuleFactory == null)
                throw new ArgumentNullException("defaultAvailabilityRuleFactory");
            if (defaultConditionRuleFactory == null) throw new ArgumentNullException("defaultConditionRuleFactory");

            _defaultOnboardRuleFactory = defaultOnboardRuleFactory;
            _defaultAvailabilityRuleFactory = defaultAvailabilityRuleFactory;
            _defaultConditionRuleFactory = defaultConditionRuleFactory;
            _ruleFactoryBuilder = ruleFactoryBuilder;
            _experiments = experiments.ToArray();
        }


        private static Maybe<ConfigNode> GetConfig(ConfigNode config, string nodeName)
        {
            return config.GetNodeEx(nodeName, false);
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


        private Maybe<IRuleFactory> BuildRuleFactoryFrom(ConfigNode config)
        {
            return _ruleFactoryBuilder.CanHandle(config) ? _ruleFactoryBuilder.Build(config).ToMaybe() : Maybe<IRuleFactory>.None;
        }


        public SensorDefinition Build(IConfigNodeObjectGraphBuilder<SensorDefinition> builder, ConfigNode config)
        {
            var experimentId = GetExperimentId(config);

            if (!experimentId.Any())
                throw new ArgumentException("config does not contain an " + ExperimentIdValueName + " entry");

            var experiment = GetExperiment(experimentId.Value);

            if (!experiment.Any())
                throw new ArgumentException(experimentId.Value + " is unrecognized");

            return new SensorDefinition(
                experiment.Value,
                GetConfig(config, OnboardRuleNodeName).With(BuildRuleFactoryFrom).Or(_defaultOnboardRuleFactory),
                GetConfig(config, AvailabilityRuleNodeName).With(BuildRuleFactoryFrom).Or(_defaultAvailabilityRuleFactory),
                GetConfig(config, ConditionRuleNodeName).With(BuildRuleFactoryFrom).Or(_defaultConditionRuleFactory));
        }


        public SensorDefinition Build(ConfigNode config)
        {
            return Build(this, config);
        }


        public SensorDefinition Create(ScienceExperiment experiment)
        {
            return new SensorDefinition(experiment, _defaultOnboardRuleFactory, _defaultAvailabilityRuleFactory,
                _defaultAvailabilityRuleFactory);
        }


        public bool CanHandle(ConfigNode config)
        {
            if (config == null) throw new ArgumentNullException("config");

            return config.name == ExperimentRulesetNodeName && 
                GetExperimentId(config)
                    .With(GetExperiment)
                    .Any();
        }


        public static class Factory // yes, a factory that creates factories. This is to isolate the logic for default rule factories
        {
            private const string DefaultOnboardRuleNodeName = "SA_DEFAULT_ONBOARD_RULE";
            private const string DefaultAvailabilityRuleNodeName = "SA_DEFAULT_AVAILABILITY_RULE";
            private const string DefaultConditionRuleNodeName = "SA_DEFAULT_CONDITION_RULE";


            private class DefaultRuleFactoryBuilder : IConfigNodeObjectGraphBuilder<IRuleFactory>
            {
                private readonly string[] _handledNodes =
                {
                    DefaultOnboardRuleNodeName, DefaultAvailabilityRuleNodeName, DefaultConditionRuleNodeName
                };


                public IRuleFactory Build(IConfigNodeObjectGraphBuilder<IRuleFactory> builder, ConfigNode config)
                {
                    if (config == null) throw new ArgumentNullException("config");

                    if (config.CountNodes == 0)
                        throw new ArgumentException("No default rule data supplied", "config");
                    if (config.CountNodes > 1)
                        throw new ArgumentException(
                            "Multiple subnodes of default rule config; did you mean to combine them with a composite rule?",
                            "config");

                    var ruleDefinition = config.nodes[0];

                    if (!builder.CanHandle(ruleDefinition))
                        throw new InvalidOperationException("Builder can't handle definition: " + ruleDefinition.name + ", check default rule ConfigNodes");

                    return builder.Build(builder, ruleDefinition);
                }


                public IRuleFactory Build(ConfigNode config)
                {
                    return Build(this, config);
                }


                public bool CanHandle(ConfigNode config)
                {
                    if (config == null) throw new ArgumentNullException("config");

                    return _handledNodes.Any(h => config.name == h);
                }
            }


            private static IRuleFactory CreateRuleFactoryFromSingleConfigNode(IGameDatabase database, IConfigNodeObjectGraphBuilder<IRuleFactory> builder, string nodeName)
            {
                if (database == null) throw new ArgumentNullException("database");
                if (builder == null) throw new ArgumentNullException("builder");
                if (string.IsNullOrEmpty(nodeName))
                    throw new ArgumentException("must specify a name", "nodeName");

                var possibilities = database.GetConfigs(nodeName);

                if (!possibilities.Any())
                    throw new ArgumentException("Failed to find any ConfigNode matching \"" + nodeName + "\"", "nodeName");

                if (possibilities.Length > 1)
                {
                    possibilities.ToList().ForEach(p => Log.Error("Found " + nodeName + " at " + p.Url));
                    throw new ArgumentException("Found multiple entries for \"" + nodeName + "\"!", "nodeName");
                }

                var cfgToBuildFactoryFrom = possibilities.Single().Config;

                return builder.Build(builder, cfgToBuildFactoryFrom);
            }


            public static SensorDefinitionFactory Create(
                IEnumerable<ScienceExperiment> experiments,
                IConfigNodeObjectGraphBuilder<IRuleFactory> ruleFactoryBuilder,
                IGameDatabase gameDatabase)
            {
                if (ruleFactoryBuilder == null) throw new ArgumentNullException("ruleFactoryBuilder");
                if (gameDatabase == null) throw new ArgumentNullException("gameDatabase");

                var defaultRuleFactoryBuilder = 
                    new CompositeConfigNodeObjectGraphBuilder<IRuleFactory>(new DefaultRuleFactoryBuilder(), ruleFactoryBuilder);

                var defaultOnboardRuleFactory = CreateRuleFactoryFromSingleConfigNode(gameDatabase,
                    defaultRuleFactoryBuilder, DefaultOnboardRuleNodeName);

                var defaultAvailabilityRuleFactory = CreateRuleFactoryFromSingleConfigNode(gameDatabase,
                    defaultRuleFactoryBuilder, DefaultAvailabilityRuleNodeName);

                var defaultConditionRuleFactory = CreateRuleFactoryFromSingleConfigNode(gameDatabase,
                    defaultRuleFactoryBuilder, DefaultConditionRuleNodeName);

                return new SensorDefinitionFactory(
                    experiments, 
                    defaultOnboardRuleFactory,
                    defaultAvailabilityRuleFactory,
                    defaultConditionRuleFactory,
                    ruleFactoryBuilder);
            }
        }
    }
}
