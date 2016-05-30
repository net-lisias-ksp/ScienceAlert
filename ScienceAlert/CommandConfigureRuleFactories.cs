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
    class CommandConfigureRuleFactories : CommandConfigureObjectFromConfigNodeBuilders<IRuleFactory>
    {
        public CommandConfigureRuleFactories(
            ITemporaryBindingFactory temporaryBinder) : base(temporaryBinder)
        {
        }

        
        protected override ReadOnlyCollection<IRuleFactory> CreateBuilders(ReadOnlyCollection<Type> builderTypes)
        {
            var logicBuilders = CreateLogicBuilders();
            var genericBuilders = CreateGenericBuilders();

            return new ReadOnlyCollection<IRuleFactory>(
                logicBuilders                               // first priority
                .Union(base.CreateBuilders(builderTypes))   // custom builders
                .Union(genericBuilders).ToList());          // generic builders
        }


        private static IEnumerable<IRuleFactory> CreateLogicBuilders()
        {
            return new IRuleFactory[]
                {
                    // "AND" logical operation
                    new DelegateRuleFactory(
                        (config, param1, param2, param3) => CreateLogicalRule(AllRulesPass, config, param1, param2, param3), "AND", "ALL", "EVERYTHING", "THESE", "ALL_OF"),

                    // "OR" logical operation (any of rules passes)
                    new DelegateRuleFactory(
                        (config, param1, param2, param3) => CreateLogicalRule(AnyRulePasses, config, param1, param2, param3), "ANY", "OR", "ANYTHING", "AT_LEAST_ONE", "ANY_OF")
                };
        }


        // rather than have a specific IRuleFactory for every type of experiment rule, we'll specify a generic builder instance for each 
        // that will try to satisfy any dependencies using the vessel context
        //
        // this generic builder will have a lower priority than any custom builder
        private IEnumerable<IRuleFactory> CreateGenericBuilders()
        {
            var ruleTypes = GetAllTypesThatImplement<ISensorRule>().ToList()
                .Where(t => t.GetConstructors(BindingFlags.Instance | BindingFlags.Public).Any())
                .ToList();

            ruleTypes.ForEach(rt => Log.Debug(rt.FullName + " will have a generic builder created"));

            return ruleTypes
                .Select(tExperimentRule => typeof (GenericRuleFactory<>).MakeGenericType(tExperimentRule))
                .Select(tGenericRuleBuilder =>
                {
                    using (var tempBinding = TemporaryBinder.Create(tGenericRuleBuilder))
                        return (IRuleFactory) tempBinding.GetInstance();
                });
        }


        // create a composite rule builder out of all builders. This hides some of the complexity: it'll look like this
        // one IRuleFactory actually builds every supported rule when internally it's finding the real builder
        protected override void BindBuildersToCrossContext(ReadOnlyCollection<IRuleFactory> builders)
        {
            base.BindBuildersToCrossContext(builders);

            var composite =
                new CompositeRuleFactory(
                    builders
                        .Cast
                        <
                            IConfigNodeObjectBuilder
                                <ISensorRule, IRuleFactory, IInjectionBinder, ITemporaryBindingFactory>>());

            injectionBinder.Bind<IRuleFactory>().To(composite).CrossContext();
        }


        // Creates a rule that uses the specified logical operation as its logic
        private static ISensorRule CreateLogicalRule(
            Func<ISensorRule[], bool> logicalOperation,
            ConfigNode config,
            IRuleFactory allFactories,
            IInjectionBinder injectionBinder,
            ITemporaryBindingFactory bindingFactory)
        {
            if (logicalOperation == null) throw new ArgumentNullException("logicalOperation");
            if (config == null) throw new ArgumentNullException("config");
            if (allFactories == null) throw new ArgumentNullException("allFactories");

            var subNodes = config.nodes.Cast<ConfigNode>().ToList();

            // make sure we can fully handle this rule (barring any unforeseen issues)
            foreach (var n in subNodes)
                if (!allFactories.CanHandle(n))
                    throw new BuilderCannotHandleConfigNodeException(n);

            var subRules =
                subNodes.Select(n => allFactories.Build(n, allFactories, injectionBinder, bindingFactory)).ToArray();

            return new CompositeRule(logicalOperation, subRules);
        }


        // used in a logical rule
        private static bool AllRulesPass(ISensorRule[] rules)
        {
            return rules.All(r => r.Passes());
        }

        // used in a logical rule
        private static bool AnyRulePasses(ISensorRule[] rules)
        {
            return rules.Any(r => r.Passes());
        }
    }
}