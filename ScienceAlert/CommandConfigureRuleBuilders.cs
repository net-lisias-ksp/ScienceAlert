using System;
using System.Collections.Generic;
using System.Linq;
using ReeperCommon.Logging;
using ScienceAlert.VesselContext.Experiments.Rules;
using ScienceAlert.VesselContext.Experiments.Trigger;

namespace ScienceAlert
{
    internal class CommandConfigureTriggerBuilders : CommandConfigureObjectFromConfigNodeBuilders
    {
        public CommandConfigureTriggerBuilders(ITemporaryBindingFactory temporaryBinder) : base(temporaryBinder)
        {
        }


        public override void Execute()
        {
            Log.TraceMessage();
            ConfigureTriggerBuilders();
        }


        private void ConfigureTriggerBuilders()
        {
            
        }
    }

// ReSharper disable once ClassNeverInstantiated.Global
    internal class CommandConfigureRuleBuilders : CommandConfigureObjectFromConfigNodeBuilders
    {
        public CommandConfigureRuleBuilders(ITemporaryBindingFactory temporaryBinder) : base(temporaryBinder)
        {
        }

        public override void Execute()
        {
            Log.TraceMessage();
            
            ConfigureRuleBuilders();
        }


        private static IExperimentRule CreateLogicalRule(
            Func<IExperimentRule[], bool> logicalOperation, 
            ConfigNode config, 
            IRuleBuilder allBuilders, 
            ITemporaryBindingFactory bindingFactory)
        {
            if (logicalOperation == null) throw new ArgumentNullException("logicalOperation");
            if (config == null) throw new ArgumentNullException("config");
            if (allBuilders == null) throw new ArgumentNullException("allBuilders");

            var subNodes = config.nodes.Cast<ConfigNode>().ToList();

            // make sure we can fully handle this rule (barring any unforeseen issues)
            foreach (var n in subNodes)
                if (!allBuilders.CanHandle(n))
                    throw new BuilderCannotHandleConfigNodeException(n);

            var subRules = subNodes.Select(n => allBuilders.Build(n, allBuilders, bindingFactory)).ToArray();

            return new CompositeRule(logicalOperation, subRules);
        }


        private static bool AllRulesPass(IExperimentRule[] rules)
        {
            return rules.All(r => r.Passes());
        }


        private static bool AnyRulePasses(IExperimentRule[] rules)
        {
            return rules.Any(r => r.Passes());
        }


        private void ConfigureRuleBuilders()
        {
            var logicBuilders = new IRuleBuilder[]
            {
                // "AND" logical operation
                new DelegateRuleBuilder(
                    (config, param1, param2) => CreateLogicalRule(AllRulesPass, config, param1, param2), "AND", "ALL", "EVERYTHING", "THESE", "ALL_OF"),

                // "OR" logical operation (any of rules passes)
                new DelegateRuleBuilder(
                    (config, param1, param2) => CreateLogicalRule(AnyRulePasses, config, param1, param2), "ANY", "OR", "ANYTHING", "AT_LEAST_ONE", "ANY_OF")
            };

            var ruleBuilders = logicBuilders.Union(
                CreateBuilderInstances<IRuleBuilder, IExperimentRule>(typeof (GenericRuleBuilder<>)))
                .ToList();

            injectionBinder
                .Bind<IEnumerable<IRuleBuilder>>()
                .Bind<List<IRuleBuilder>>()
                .To(ruleBuilders)
                .CrossContext();

            // these builders can be nested so it's convenient to have a composite builder which hides the complexity
            // and makes it look like there's just one builder that knows everything
            injectionBinder
                .Bind<IRuleBuilder>()
                .To(
                    new CompositeRuleBuilder(
                        ruleBuilders
                            .Cast
                            <IConfigNodeObjectBuilder<IExperimentRule, IRuleBuilder, ITemporaryBindingFactory>>()))
                .CrossContext();
        }
    }
}