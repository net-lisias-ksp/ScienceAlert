using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using JetBrains.Annotations;
using ReeperCommon.Containers;
using ReeperCommon.Logging;
using ReeperKSP.Extensions;
using strange.extensions.injector.api;

namespace ScienceAlert.VesselContext.Experiments.Sensors.Rules
{
    /// <summary>
    /// Will try to build any rule by satisfying its needs with dependency injection. Custom rules might
    /// need to provide their own builder
    /// </summary>
    [RegisterBuilder(typeof(IObjectFromConfigNodeBuilder<ISensorRule, ConfigNode, IInjectionBinder>), int.MaxValue)]
    // ReSharper disable once UnusedMember.Global
    public class GenericRuleBuilder : IObjectFromConfigNodeBuilder<ISensorRule, ConfigNode, IInjectionBinder>
    {
        private readonly string[] _namesMatchingAndRule = { "AND", "ALL", "ALL_OF", "REQUIRE", "REQUIRED" };
        private readonly string[] _namesMatchingOrRule = { "OR", "ANY", "ANY_OF", "ONE_OF" };

        private readonly ReadOnlyCollection<Type> _sensorRuleTypes;
        private readonly ITemporaryBindingFactory _tempBinder;
        private readonly Lazy<string[]> _logicalRuleNames; 

        private class LogicalDelegateRule : ISensorRule
        {
            private readonly Func<ISensorRule[], bool> _strategy;
            private readonly ISensorRule[] _subRules;

            public LogicalDelegateRule([NotNull] Func<ISensorRule[], bool> strategy, [NotNull] ISensorRule[] subRules)
            {
                if (strategy == null) throw new ArgumentNullException("strategy");
                if (subRules == null) throw new ArgumentNullException("subRules");
                _strategy = strategy;
                _subRules = subRules;

                Log.Warning("LogicalDelegateRule with " + strategy.Method.Name);
                foreach (var r in subRules)
                    Log.Warning("   - subrule: " + r.GetType().Name);
            }

            public bool Passes()
            {
                return _strategy(_subRules);
            }
        }

        public GenericRuleBuilder(
            [NotNull, Name(CrossContextKeys.SensorRuleTypes)] ReadOnlyCollection<Type> sensorRuleTypes,
            [NotNull] ITemporaryBindingFactory tempBinder)
        {
            if (sensorRuleTypes == null) throw new ArgumentNullException("sensorRuleTypes");
            if (tempBinder == null) throw new ArgumentNullException("tempBinder");
            _sensorRuleTypes = sensorRuleTypes;
            _tempBinder = tempBinder;
            _logicalRuleNames = new Lazy<string[]>(() => _namesMatchingOrRule.Union(_namesMatchingAndRule).ToArray());
        }


        /// <exception cref="UnrecognizedRuleException">Condition.</exception>
        /// <exception cref="BuilderCannotHandleConfigNodeException">ConfigNode specifies an unrecognized rule name in a node or subnode.</exception>
        public ISensorRule Build(ConfigNode config, IInjectionBinder binder, IObjectFromConfigNodeBuilder<ISensorRule, ConfigNode, IInjectionBinder> rootBuilder = null)
        {
            if (!CanHandle(config, binder, rootBuilder))
                throw new BuilderCannotHandleConfigNodeException(config);

            var ruleName = config.name;
            var rule = GetRuleByName(ruleName);

            if (!rule.Any())
                throw new UnrecognizedRuleException(ruleName);

            var subRules =
                config.nodes.Cast<ConfigNode>()
                    .Select(subRuleConfig => (rootBuilder ?? this).Build(subRuleConfig, binder, (rootBuilder ?? this)))
                    .ToList();

            if (rule.Value == typeof (LogicalDelegateRule))
                return CreateLogicalRule(config, subRules);

            using (var bound = _tempBinder.Create(binder, rule.Value))
                return (ISensorRule) bound.GetInstance();
        }


        private ISensorRule CreateLogicalRule(ConfigNode config, List<ISensorRule> subRules)
        {
            var logicalOperator = GetLogicalRuleDelegateFor(config);

            Log.Warning("For " + config.name + ", " + logicalOperator.Method.Name + " will be used with " + subRules.Count + " subRules");

            return new LogicalDelegateRule(logicalOperator, subRules.ToArray());
        }


        private Func<ISensorRule[], bool> GetLogicalRuleDelegateFor(ConfigNode config)
        {
            if (_namesMatchingOrRule.Any(
                    orRuleName => orRuleName.Equals(config.name, StringComparison.OrdinalIgnoreCase)))
                return LogicalOr;

            if (_namesMatchingAndRule.Any(
                andRuleName => andRuleName.Equals(config.name, StringComparison.OrdinalIgnoreCase)))
                return LogicalAnd;

            throw new ArgumentException("Unrecognized logical rule: " + config.name, "config"); // shouldn't be possible but just in case
        }

        public bool CanHandle(ConfigNode config, IInjectionBinder param2, IObjectFromConfigNodeBuilder<ISensorRule, ConfigNode, IInjectionBinder> rootBuilder = null)
        {
            if (config == null) return false;

            var builder = rootBuilder ?? this;
            var root = GetRuleByName(config.name);

            if (!root.Any()) return false;

            // as long as all subrules are also handled, it's good
            return config.nodes
                .Cast<ConfigNode>()
                .All(subRule => builder.CanHandle(subRule, param2, builder));
        }


        private Maybe<Type> GetRuleByName(string name)
        {
            if (string.IsNullOrEmpty(name))
                return Maybe<Type>.None;

            foreach (var rule in _sensorRuleTypes)
                if (string.CompareOrdinal(name, rule.Name) == 0)
                    return rule.ToMaybe();

            foreach (var logicalRuleName in _logicalRuleNames.Value)
                if (string.CompareOrdinal(name, logicalRuleName) == 0)
                    return typeof (LogicalDelegateRule).ToMaybe();

            return Maybe<Type>.None;
        }


        private static bool LogicalAnd(ISensorRule[] sensors)
        {
            foreach (var s in sensors)
                if (!s.Passes()) return false;
            return true;
        }

        private static bool LogicalOr(ISensorRule[] sensors)
        {
            foreach (var s in sensors)
                if (s.Passes()) return true;
            return sensors.Length != 0; // empty OR rule will pass
        }
    }
}
