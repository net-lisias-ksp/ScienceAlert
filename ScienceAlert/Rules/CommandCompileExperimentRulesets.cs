using System;
using System.Collections.Generic;
using System.Linq;
using ReeperCommon.Containers;
using ReeperCommon.Extensions;
using ScienceAlert.Core;
using strange.extensions.command.impl;
using strange.extensions.injector;

namespace ScienceAlert.Rules
{
// ReSharper disable once ClassNeverInstantiated.Global
    public class CommandCompileExperimentRulesets : Command
    {
        private const string OnboardRuleDefinitionNodeName = "ONBOARD_RULE";
        private const string AvailabilityRuleDefinitionNodeName = "AVAILABILITY_RULE";

        private readonly RuleDefinitionFactory _ruleDefinitionFactory;
        private readonly IEnumerable<ScienceExperiment> _experiments;
        private readonly IEnumerable<ConfigNode> _ruleConfigs;

        public CommandCompileExperimentRulesets(
            RuleDefinitionFactory ruleDefinitionFactory,
            IEnumerable<ScienceExperiment> experiments,
            [Name(CoreKeys.ExperimentRuleConfigs)] IEnumerable<ConfigNode> ruleConfigs)
        {
            if (ruleDefinitionFactory == null) throw new ArgumentNullException("ruleDefinitionFactory");
            if (experiments == null) throw new ArgumentNullException("experiments");
            if (ruleConfigs == null) throw new ArgumentNullException("ruleConfigs");

            _ruleDefinitionFactory = ruleDefinitionFactory;
            _experiments = experiments;
            _ruleConfigs = ruleConfigs;
        }


        public override void Execute()
        {
            var compiledRules = new Dictionary<ScienceExperiment, ExperimentRuleset>();

            foreach (var cfg in _ruleConfigs)
            {
                var ruleset = CompileRuleset(cfg);

                if (!ruleset.Any())
                    continue;

                if (compiledRules.ContainsKey(ruleset.Value.Experiment))
                {
                    Log.Error("Multiple rule definitions found for " + GetExperimentID(cfg) +
                                "; this one will be ignored");
                    continue;
                }


                compiledRules.Add(ruleset.Value.Experiment, ruleset.Value);
            }

            Log.Verbose("Compiled " + compiledRules.Values.Count + " experiment rulesets");

            foreach (var ruleset in compiledRules.Values)
                injectionBinder.Bind<ExperimentRuleset>().ToValue(ruleset).ToName(ruleset.Experiment).CrossContext();

            injectionBinder.Bind<IEnumerable<ExperimentRuleset>>().ToValue(compiledRules.Values.ToList()).CrossContext();
        }


        private Maybe<ExperimentRuleset> CompileRuleset(ConfigNode ruleConfig)
        {
            try
            {
                var forExperiment = GetRelatedExperiment(ruleConfig);

                if (!forExperiment.Any())
                {
                    Log.Verbose("Ignoring rule definition for " + GetExperimentID(ruleConfig) +
                                " because no related ScienceExperiment exists");
                    return Maybe<ExperimentRuleset>.None;
                }

                var onboardDefinition = CreateOnboardRuleDefinition(ruleConfig);
                var availabilityDefinition = CreateAvailabilityRuleDefinition(ruleConfig);

                return new ExperimentRuleset(forExperiment.Value, onboardDefinition, availabilityDefinition).ToMaybe();
            }
            catch (Exception e)
            {
                if (e is CompositeRuleEmptyException || e is RuleDefinitionMissingExperimentIdException ||
                    e is RuleTypeNotFoundException || e is DuplicateConfigNodeSectionException)
                {
                    Log.Error("Failed to compile rule for " + GetExperimentID(ruleConfig) + ": " + e);
                    return Maybe<ExperimentRuleset>.None;
                }
                else throw;
            }
        }


        private Maybe<ScienceExperiment> GetRelatedExperiment(ConfigNode config)
        {
            if (config == null) throw new ArgumentNullException("config");

            var targetId = GetExperimentID(config);

            return _experiments.FirstOrDefault(se => se.id == targetId).ToMaybe();
        }


// ReSharper disable once InconsistentNaming
        private static string GetExperimentID(ConfigNode config)
        {
            if (config == null) throw new ArgumentNullException("config");

            var targetId = config.GetValue("experimentID");

            if (!config.HasValue("experimentID") || string.IsNullOrEmpty(targetId))
                throw new RuleDefinitionMissingExperimentIdException();

            return targetId;
        }


        private static RuleDefinition CreateDefaultOnboardRuleDefinition()
        {
            return new RuleDefinition(typeof (VesselHasModuleScienceExperiment));
        }


        private static RuleDefinition CreateDefaultAvailabilityRuleDefinition()
        {
            return new RuleDefinition(typeof (VesselHasModuleScienceExperiment));
        }


        private RuleDefinition CreateOnboardRuleDefinition(ConfigNode config)
        {
            if (!config.HasNode(OnboardRuleDefinitionNodeName))
                return CreateDefaultOnboardRuleDefinition();

            var onboardRules = config.GetNodes(OnboardRuleDefinitionNodeName);

            if (onboardRules.Length > 1)
                throw new DuplicateConfigNodeSectionException(OnboardRuleDefinitionNodeName);

            ConfigNode rule = null;

            if (onboardRules.Single().CountNodes == 0)
            {
                Log.Debug("Onboard rule empty; creating default");
                return CreateDefaultOnboardRuleDefinition();
            }

            if (onboardRules.Single().CountNodes == 1)
                rule = onboardRules.Single().nodes[0];
            else if (onboardRules.Single().CountNodes > 1)
            {
                Log.Debug("Multiple onboard rules found; creating a composite AND rule that includes all of them");
                var compositeAll = new ConfigNode(RuleDefinitionFactory.CompositeAllName);
                onboardRules.Single().CopyTo(compositeAll);

                Log.Debug("Result: " + compositeAll.ToSafeString());
            }

            return _ruleDefinitionFactory.Create(rule);
        }


        private RuleDefinition CreateAvailabilityRuleDefinition(ConfigNode config)
        {
            if (!config.HasNode(AvailabilityRuleDefinitionNodeName))
                return CreateDefaultAvailabilityRuleDefinition();

            var availabilityRules = config.GetNodes(AvailabilityRuleDefinitionNodeName);

            if (availabilityRules.Length > 1)
                throw new DuplicateConfigNodeSectionException(AvailabilityRuleDefinitionNodeName);

            return _ruleDefinitionFactory.Create(availabilityRules.Single()); 
        }
    }
}
