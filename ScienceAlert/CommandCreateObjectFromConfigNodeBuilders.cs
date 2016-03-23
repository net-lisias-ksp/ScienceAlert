using System;
using System.Collections.Generic;
using System.Linq;
using ReeperCommon.Containers;
using ReeperCommon.Logging;
using strange.extensions.command.impl;
using ScienceAlert.VesselContext.Experiments.Rules;
using ScienceAlert.VesselContext.Experiments.Trigger;

namespace ScienceAlert
{

// ReSharper disable once ClassNeverInstantiated.Global
    internal class CommandCreateObjectFromConfigNodeBuilders : Command
    {
        private readonly ITemporaryBindingFactory _temporaryBinder;
        private readonly Lazy<IEnumerable<Type>> _allAssemblyTypes;

        public CommandCreateObjectFromConfigNodeBuilders(ITemporaryBindingFactory temporaryBinder)
        {
            if (temporaryBinder == null) throw new ArgumentNullException("temporaryBinder");

            _temporaryBinder = temporaryBinder;
            _allAssemblyTypes = new Lazy<IEnumerable<Type>>(GetAllTypesFromLoadedAssemblies);
        }


        private static IEnumerable<Type> GetAllTypesFromLoadedAssemblies()
        {
            return AssemblyLoader.loadedAssemblies
                .SelectMany(la => la.assembly.GetTypes());
        }

        private static bool IsConcreteInstanceOf(Type baseType, Type queryType)
        {
            return !queryType.IsAbstract && !queryType.IsGenericTypeDefinition &&
                   baseType.IsAssignableFrom(queryType);
        }

        private IEnumerable<Type> GetAllConcreteTypesAssignableFrom<T>()
        {
// ReSharper disable once HeapView.SlowDelegateCreation
            return _allAssemblyTypes.Value.Where(t => IsConcreteInstanceOf(typeof (T), t));
        }


        public override void Execute()
        {
            Log.TraceMessage();

            ConfigureRuleBuilders();
            ConfigureTriggerBuilders();
        }


        private static IExperimentRule CreateLogicalRule(Func<IExperimentRule[], bool> logicalOperation, ConfigNode config, IRuleBuilder allBuilders, ITemporaryBindingFactory bindingFactory)
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
                new DelegateFactoryBuilder<IExperimentRule, IRuleBuilder, ITemporaryBindingFactory>(
                    (config, param1, param2) =>
                    {
                        return (IExperimentRule)null;
                    })
            };

            var ruleBuilders = logicBuilders.Union(
                CreateBuilders<IRuleBuilder, IExperimentRule>(typeof (GenericRuleBuilder<>)))
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
                        // Add builders for the logical rules
                        new[]
                        {

                        }
                            .Union(
                                ruleBuilders
                                    .Cast
                                    <IConfigNodeObjectBuilder<IExperimentRule, IRuleBuilder, ITemporaryBindingFactory>>())))
                .CrossContext();
        }


        private void ConfigureTriggerBuilders()
        {
            var triggerBuilders =
                CreateBuilders<ITriggerBuilder, ExperimentTrigger>(typeof (GenericTriggerBuilder<>)).ToList();

            injectionBinder
                .Bind<IEnumerable<ITriggerBuilder>>()
                .Bind<List<ITriggerBuilder>>()
                .To(triggerBuilders);
        }


        private IEnumerable<TBuilderInterface> CreateBuilders<TBuilderInterface, TThingBuilderBuilders>(
            Type genericBuilderTypeDefinition)
        {
            var builderInstances = new List<TBuilderInterface>();
            var builderTypes =
                GetConcreteBuilderTypes<TBuilderInterface, TThingBuilderBuilders>(genericBuilderTypeDefinition)
                    .ToList();

            // bind the types first in case a builder has a dependency on another builder
            builderTypes.ForEach(builderType => injectionBinder.Bind(builderType).CrossContext());

            foreach (var ty in builderTypes)
            {
                try
                {
                    var instance = CreateBuilder<TBuilderInterface>(ty);

                    builderInstances.Add(instance);
                }
                catch (Exception e)
                {
                    Log.Warning("Could not create " + typeof (TBuilderInterface).Name + " of type " + ty.FullName +
                                " due to: " + e);
                }
            }

            return builderInstances;
        }

        private TReturnType CreateBuilder<TReturnType>(Type builderType)
        {
            if (builderType == null) throw new ArgumentNullException("builderType");
            if (builderType.IsAbstract || builderType.IsGenericTypeDefinition)
                throw new ArgumentException("Cannot create " + builderType);

            if (!typeof (TReturnType).IsAssignableFrom(builderType))
                throw new ArgumentException(builderType + " does not implement " + typeof (TReturnType).Name);

            using (var builderBinding = _temporaryBinder.Create(builderType))
            {
                var returnValue = (TReturnType) builderBinding.GetInstance();

                if (returnValue == null)
                    throw new ArgumentException("Unable to create " + builderType);

                return returnValue;
            }
        }


        private IEnumerable<Type> GetConcreteBuilderTypes<TTypeThatMarksBuilders, TTypeThatMarksThingsThatAreBuilt>(
            Type genericBuilderTypeDefinition)
        {
            if (!genericBuilderTypeDefinition.IsGenericTypeDefinition)
                throw new ArgumentException(genericBuilderTypeDefinition.Name + " must be the generic version");

            var concreteBuilderTypes = GetAllConcreteTypesAssignableFrom<TTypeThatMarksBuilders>().ToList();
            var allTypesThatMightBeBuilt =
                GetAllConcreteTypesAssignableFrom<TTypeThatMarksThingsThatAreBuilt>().ToList();

            // create the generic builder for each type that might be built
            var genericBuilders =
                allTypesThatMightBeBuilt
                    // ReSharper disable once HeapView.DelegateAllocation
                    // ReSharper disable once HeapView.SlowDelegateCreation
                    .Select(t => genericBuilderTypeDefinition.MakeGenericType(t));

            return concreteBuilderTypes.Union(genericBuilders).Where(t => !t.Name.StartsWith("Composite"));
        }
    }
}