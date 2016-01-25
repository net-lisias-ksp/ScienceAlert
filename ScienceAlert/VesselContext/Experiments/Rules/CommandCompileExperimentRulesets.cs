using System;
using System.Collections.Generic;
using System.Linq;
using ReeperCommon.Containers;
using ReeperCommon.Extensions;
using ScienceAlert.Core;
using strange.extensions.command.impl;

namespace ScienceAlert.VesselContext.Experiments.Rules
{
// ReSharper disable once ClassNeverInstantiated.Global
    public class CommandCompileExperimentRulesets : Command
    {
        private const string OnboardRuleDefinitionNodeName = "ONBOARD_RULE";
        private const string AvailabilityRuleDefinitionNodeName = "AVAILABILITY_RULE";
// ReSharper disable once InconsistentNaming
        private const string ExperimentIDFieldName = "experimentID";

        private readonly RuleDefinitionFactory _ruleDefinitionFactory;
        private readonly IEnumerable<ScienceExperiment> _experiments;
        private readonly IEnumerable<ConfigNode> _ruleConfigs;

        private readonly Lazy<RuleDefinition> _defaultOnboardRule =
            new Lazy<RuleDefinition>(CreateDefaultOnboardRuleDefinition);

        private readonly Lazy<RuleDefinition> _defaultAvailabilityRule =
            new Lazy<RuleDefinition>(CreateDefaultAvailabilityRuleDefinition);
 

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
            var definedRulesets = CompileRulesetsFromConfigNodes();
            var defaultRulesets =
                CompileDefaultRulesetsForRemainingExperiments(definedRulesets.Select(r => r.Experiment));

            var combinedRules = new List<ExperimentRuleset>(definedRulesets.Union(defaultRulesets));

            Log.Verbose("Compiled " + definedRulesets.Count + " defined experiment rulesets");
            Log.Verbose("Compiled " + defaultRulesets.Count + " default experiment rulesets");

            foreach (var ruleset in combinedRules)
                injectionBinder.Bind<ExperimentRuleset>().ToValue(ruleset).ToName(ruleset.Experiment).CrossContext();

            injectionBinder.Bind<IEnumerable<ExperimentRuleset>>().ToValue(combinedRules).CrossContext();
        }


        private IList<ExperimentRuleset> CompileRulesetsFromConfigNodes()
        {
            var rulesets = new List<ExperimentRuleset>();

            foreach (var cfg in _ruleConfigs)
            {
                var ruleset = CompileRuleset(cfg);
                var experimentId = GetExperimentID(cfg);

                if (!ruleset.Any())
                    continue;

                if (rulesets.Any(rs => ReferenceEquals(rs.Experiment, ruleset.Value.Experiment)))
                {
                    Log.Error("Multiple rule definitions found for " + experimentId +
                                "; this one will be ignored");
                    continue;
                }

                rulesets.Add(ruleset.Value);
                Log.Debug("Compiled ruleset for " + experimentId);
            }

            
            return rulesets;
        }


        private IList<ExperimentRuleset> CompileDefaultRulesetsForRemainingExperiments(
            IEnumerable<ScienceExperiment> experimentsWithRules)
        {
            return
                _experiments.Except(experimentsWithRules)
                    .Select(se => new ExperimentRuleset(se, _defaultOnboardRule.Value, _defaultAvailabilityRule.Value)).ToList();
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
                if (e is CompositeRuleEmptyException || e is RulesetMissingExperimentIdException ||
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

            var targetId = config.GetValueEx(ExperimentIDFieldName, false);

            if (!targetId.Any() || string.IsNullOrEmpty(targetId.Value))
            {
                Log.Error("Ruleset missing ExperimentID value: " + config.ToSafeString());
                throw new RulesetMissingExperimentIdException();
            }

            return targetId.Single();
        }


        private static RuleDefinition CreateDefaultOnboardRuleDefinition()
        {
            return new RuleDefinition(typeof (VesselHasExperimentModuleScienceExperimentExperiment));
        }


        private static RuleDefinition CreateDefaultAvailabilityRuleDefinition()
        {
            return new RuleDefinition(typeof (VesselHasExperimentModuleScienceExperimentExperiment));
        }



        private RuleDefinition CreateOnboardRuleDefinition(ConfigNode config)
        {
            if (!config.HasNode(OnboardRuleDefinitionNodeName))
                return _defaultOnboardRule.Value;

            var onboardRules = config.GetNodes(OnboardRuleDefinitionNodeName);

            if (onboardRules.Length > 1)
                throw new DuplicateConfigNodeSectionException(OnboardRuleDefinitionNodeName);


            if (onboardRules.Single().CountNodes != 0) return CreateRules(onboardRules.Single());

            Log.Debug("Onboard rule empty; using default");
            return _defaultOnboardRule.Value;
        }



        private RuleDefinition CreateAvailabilityRuleDefinition(ConfigNode config)
        {
            if (!config.HasNode(AvailabilityRuleDefinitionNodeName))
                return _defaultAvailabilityRule.Value;

            var availabilityRules = config.GetNodes(AvailabilityRuleDefinitionNodeName);

            if (availabilityRules.Length > 1)
                throw new DuplicateConfigNodeSectionException(AvailabilityRuleDefinitionNodeName);

            if (availabilityRules.Single().CountNodes != 0) return CreateRules(availabilityRules.Single());

            Log.Debug("Availability rule empty; using default");
            return _defaultAvailabilityRule.Value;
        }



        private RuleDefinition CreateRules(ConfigNode ruleset)
        {
            if (ruleset == null) throw new ArgumentNullException("ruleset");
            if (!ruleset.HasData) throw new ArgumentException("ruleset must contain data", "ruleset");

            ConfigNode rule = null;

            if (ruleset.CountNodes == 1)
                rule = ruleset.nodes[0];
            else if (ruleset.CountNodes > 1)
            {
                Log.Debug(() => "Multiple rules found; creating a composite AND rule that includes all of them");
                Log.Debug(() => ruleset.ToSafeString());

                var compositeAll = new ConfigNode(RuleDefinitionFactory.CompositeAllName);
                ruleset.CopyTo(compositeAll);

                Log.Debug("Result: " + compositeAll.ToSafeString());
            }

            return _ruleDefinitionFactory.Create(rule);
        }
    }
}
