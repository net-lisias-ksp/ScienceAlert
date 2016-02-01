//using System;
//using System.Collections.Generic;
//using System.Linq;
//using ReeperCommon.Containers;

//namespace ScienceAlert.VesselContext.Experiments.Rules.Old
//{
//    public class RuleDefinition
//    {
//        public enum DefinitionType
//        {
//            Rule,               // This RuleDefinition node represents one single rule
//            CompositeAll        // Combination of several rules, all of which must pass
//        }

   
//        public readonly DefinitionType Type = DefinitionType.Rule;
//        public readonly List<RuleDefinition> Rules = new List<RuleDefinition>();
//        public readonly Maybe<Type> Rule = Maybe<Type>.None;
//        public readonly Maybe<ConfigNode> RuleConfig = Maybe<ConfigNode>.None;


//        public RuleDefinition(DefinitionType definitionType, IEnumerable<RuleDefinition> rules)
//        {
//            Type = definitionType;
//            Rules.AddRange(rules ?? Enumerable.Empty<RuleDefinition>());
//        }


//        public RuleDefinition(Type rule, ConfigNode config = null)
//        {
//            if (rule == null) throw new ArgumentNullException("rule");

//            Type = DefinitionType.Rule;
//            Rule = rule.ToMaybe();
//            RuleConfig = config.ToMaybe();
//        }
//    }
//}
