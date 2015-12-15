using System;
using System.Collections.Generic;
using System.Linq;
using ReeperCommon.Containers;
using strange.extensions.injector.api;

namespace ScienceAlert.Rules
{
    [Implements(typeof(IExperimentRuleTypeProvider), InjectionBindingScope.CROSS_CONTEXT)]
// ReSharper disable once UnusedMember.Global
    public class ExperimentRuleTypeProvider : IExperimentRuleTypeProvider
    {
        private readonly Lazy<IEnumerable<Type>> _ruleTypes = new Lazy<IEnumerable<Type>>(CreateRuleTypeList);

        public ExperimentRuleTypeProvider()
        {
            Log.Warning("ExperimentRuleTypeProvider created");
        }


        public IEnumerable<Type> Get()
        {
            return _ruleTypes.Value;
        }


        private static IEnumerable<Type> CreateRuleTypeList()
        {
            return AssemblyLoader.loadedAssemblies
                .SelectMany(la => la.assembly.GetTypes())
                .Where(t => typeof (IExperimentRule).IsAssignableFrom(t) && !t.IsAbstract)
                .ToList();
        }
    }
}
