using System;
using System.Collections.Generic;
using System.Linq;
using ReeperCommon.Extensions;

namespace ScienceAlert.Rules
{
    public class RuleDefinitionFactory
    {
        private readonly IExperimentRuleTypeProvider _ruleTypeProvider;
        public const string CompositeAllName = "ALL_OF";
        
        private readonly Dictionary<string, Func<ConfigNode, RuleDefinition>> _ruleOperations =
            new Dictionary<string, Func<ConfigNode, RuleDefinition>>();
 
 
        public RuleDefinitionFactory(IExperimentRuleTypeProvider ruleTypeProvider)
        {
            if (ruleTypeProvider == null) throw new ArgumentNullException("ruleTypeProvider");
            _ruleTypeProvider = ruleTypeProvider;

            SetupRuleOperations();
        }


        private void SetupRuleOperations()
        {
            _ruleOperations.Add(CompositeAllName, CreateCompositeAllRule);
        }


        private RuleDefinition CreateRule(ConfigNode config)
        {
            if (config == null) throw new ArgumentNullException("config");

            var ruleType = _ruleTypeProvider.Get().FirstOrDefault(t => t.Name == config.name || t.FullName == config.name)
                           ?? Type.GetType(config.name, false);

            if (ruleType == null)
                throw new RuleTypeNotFoundException(config.name);

            if (!typeof(IExperimentRule).IsAssignableFrom(ruleType))
                throw new RuleMustImplementCorrectInterfaceException(ruleType);

            return new RuleDefinition(ruleType, config);
        }


        private RuleDefinition CreateCompositeAllRule(ConfigNode config)
        {
            if (config == null) throw new ArgumentNullException("config");
            if (!config.HasData || config.CountNodes == 0)
                throw new CompositeRuleEmptyException();

            return new RuleDefinition(RuleDefinition.DefinitionType.CompositeAll,
                config.GetNodes().Select(Create).ToList());
        }


        public RuleDefinition Create(ConfigNode config)
        {
            if (config == null) throw new ArgumentNullException("config");

            if (string.IsNullOrEmpty(config.name))
                throw new ArgumentException("ConfigNode has no name", "config");

            var ruleNodeTypeOrRuleType = config.name;
            Func<ConfigNode, RuleDefinition> operation;

            return _ruleOperations.TryGetValue(ruleNodeTypeOrRuleType, out operation) ? operation(config) : CreateRule(config);
        }
    }
}