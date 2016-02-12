using System;
using System.Collections.Generic;
using System.Linq;
using ScienceAlert.VesselContext.Experiments.Rules;
using strange.extensions.command.impl;

namespace ScienceAlert.Core
{
// ReSharper disable once ClassNeverInstantiated.Global
    class CommandStoreExperimentRuleTypes : Command
    {
        public override void Execute()
        {
            injectionBinder.Bind<IEnumerable<Type>>()
                .To(AssemblyLoader.loadedAssemblies
                    .SelectMany(la => la.assembly.GetTypes())
                    .Where(t => typeof (IExperimentRule).IsAssignableFrom(t) && !t.IsAbstract)
                    .ToList())
                .ToName(CoreKeys.ExperimentRuleTypes)
                .CrossContext();;
        }
    }
}
