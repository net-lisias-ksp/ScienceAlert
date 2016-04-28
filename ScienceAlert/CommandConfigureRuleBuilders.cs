using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using ReeperCommon.Logging;
using strange.extensions.injector.api;
using ScienceAlert.VesselContext.Experiments.Rules;

namespace ScienceAlert
{
    // ReSharper disable once ClassNeverInstantiated.Global
    class CommandConfigureRuleBuilders : CommandConfigureObjectFromConfigNodeBuilders<IRuleBuilder, IExperimentRule>
    {
        public CommandConfigureRuleBuilders(
            ITemporaryBindingFactory temporaryBinder) : base(temporaryBinder)
        {
        }

        
        protected override ReadOnlyCollection<IRuleBuilder> CreateBuilders(ReadOnlyCollection<Type> builderTypes)
        {
            var logicBuilders = CreateLogicBuilders();
            var genericBuilders = CreateGenericBuilders();

            return new ReadOnlyCollection<IRuleBuilder>(
                logicBuilders                               // first priority
                .Union(base.CreateBuilders(builderTypes))   // custom builders
                .Union(genericBuilders).ToList());          // generic builders
        }


        private static IEnumerable<IRuleBuilder> CreateLogicBuilders()
        {
            return new IRuleBuilder[]
                {
                    // "AND" logical operation
                    new DelegateRuleBuilder(
                        (config, param1, param2, param3) => CreateLogicalRule(AllRulesPass, config, param1, param2, param3), "AND", "ALL", "EVERYTHING", "THESE", "ALL_OF"),

                    // "OR" logical operation (any of rules passes)
                    new DelegateRuleBuilder(
                        (config, param1, param2, param3) => CreateLogicalRule(AnyRulePasses, config, param1, param2, param3), "ANY", "OR", "ANYTHING", "AT_LEAST_ONE", "ANY_OF")
                };
        }


        // rather than have a specific IRuleBuilder for every type of experiment rule, we'll specify a generic builder instance for each 
        // that will try to satisfy any dependencies using the vessel context
        //
        // this generic builder will have a lower priority than any custom builder
        private IEnumerable<IRuleBuilder> CreateGenericBuilders()
        {
            var ruleTypes = GetAllTypesThatImplement<IExperimentRule>().ToList()
                .Where(t => t.GetConstructors(BindingFlags.Instance | BindingFlags.Public).Any())
                .ToList();

            ruleTypes.ForEach(rt => Log.Debug(rt.FullName + " will have a generic builder created"));

            return ruleTypes
                .Select(tExperimentRule => typeof (GenericRuleBuilder<>).MakeGenericType(tExperimentRule))
                .Select(tGenericRuleBuilder =>
                {
                    using (var tempBinding = TemporaryBinder.Create(tGenericRuleBuilder))
                        return (IRuleBuilder) tempBinding.GetInstance();
                });
        }


        // create a composite rule builder out of all builders. This hides some of the complexity: it'll look like this
        // one IRuleBuilder actually builds every supported rule when internally it's finding the real builder
        protected override void BindBuildersToCrossContext(ReadOnlyCollection<IRuleBuilder> builders)
        {
            base.BindBuildersToCrossContext(builders);

            var composite =
                new CompositeRuleBuilder(
                    builders
                        .Cast
                        <
                            IConfigNodeObjectBuilder
                                <IExperimentRule, IRuleBuilder, IInjectionBinder, ITemporaryBindingFactory>>());

            injectionBinder.Bind<IRuleBuilder>().To(composite).CrossContext();
        }


        // Creates a rule that uses the specified logical operation as its logic
        private static IExperimentRule CreateLogicalRule(
            Func<IExperimentRule[], bool> logicalOperation,
            ConfigNode config,
            IRuleBuilder allBuilders,
            IInjectionBinder injectionBinder,
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

            var subRules =
                subNodes.Select(n => allBuilders.Build(n, allBuilders, injectionBinder, bindingFactory)).ToArray();

            return new CompositeRule(logicalOperation, subRules);
        }


        // used in a logical rule
        private static bool AllRulesPass(IExperimentRule[] rules)
        {
            return rules.All(r => r.Passes());
        }

        // used in a logical rule
        private static bool AnyRulePasses(IExperimentRule[] rules)
        {
            return rules.Any(r => r.Passes());
        }
    }
}