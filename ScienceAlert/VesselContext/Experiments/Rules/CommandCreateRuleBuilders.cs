using System;
using System.Collections.Generic;
using System.Linq;
using ReeperCommon.Containers;
using ReeperCommon.Logging;
using strange.extensions.command.impl;

namespace ScienceAlert.VesselContext.Experiments.Rules
{
// ReSharper disable once ClassNeverInstantiated.Global
    class CommandCreateRuleBuilders : Command
    {
        private readonly ITemporaryBindingFactory _temporaryBinder;
        private readonly Lazy<IEnumerable<Type>> _allAssemblyTypes;
  
        public CommandCreateRuleBuilders(ITemporaryBindingFactory temporaryBinder)
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

        private IEnumerable<Type> GetAllTypesAssignableFrom<T>()
        {
// ReSharper disable once HeapView.SlowDelegateCreation
            return _allAssemblyTypes.Value.Where(t => IsConcreteInstanceOf(typeof(T), t));
        }


        public override void Execute()
        {
            Log.TraceMessage();

            var concreteBuilderTypes = GetAllTypesAssignableFrom<IRuleBuilder>().ToList();
            var concreteBuilders = concreteBuilderTypes.Select(Create).ToList();
            var defaultGenericBuilder = typeof (StandardRuleBuilder<>);
            var genericBuilders = GetAllTypesAssignableFrom<IExperimentRule>()
                .Select(rt => Create(defaultGenericBuilder.MakeGenericType(new[] {rt}))).ToList();

            // Note how concrete builders go first -- this way, any builder specialized for any particular rule type will be given first chance to handle it
            // also note that the logical node builders (CompositeAndRuleBuilder for example) don't get any special attention; that's because they're
            // concrete and will already be in our list
            var allBuilders = concreteBuilders.Union(genericBuilders).ToList();
            var builderProvider = new RuleBuilderProvider(allBuilders);

            allBuilders.ForEach(builderProvider.AddBuilder);

            foreach (var b in concreteBuilders)
                Log.Debug("Concrete rule builder: " + b);

            foreach (var b in genericBuilders)
                Log.Debug("Generic rule builder: " + b);

            Log.Verbose("Created " + allBuilders.Count + " rule builders");

            injectionBinder.Bind<IEnumerable<IRuleBuilder>>().To(allBuilders).CrossContext();
            injectionBinder.Bind<IRuleBuilderProvider>().To(builderProvider).CrossContext();
        }


        private IRuleBuilder Create(Type ruleBuilderType)
        {
            if (ruleBuilderType == null) throw new ArgumentNullException("ruleBuilderType");
            if (ruleBuilderType.IsAbstract || ruleBuilderType.IsGenericTypeDefinition)
                throw new ArgumentException("Cannot create " + ruleBuilderType);
            if (!typeof (IRuleBuilder).IsAssignableFrom(ruleBuilderType))
                throw new ArgumentException(ruleBuilderType + " does not implement " + typeof (IRuleBuilder).Name);

            using (var builderBinding = _temporaryBinder.Create(ruleBuilderType))
                return (IRuleBuilder)builderBinding.GetInstance();
        }
    }
}
