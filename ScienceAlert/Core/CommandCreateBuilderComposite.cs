using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using ReeperCommon.Containers;
using ReeperCommon.Extensions;
using ReeperCommon.Logging;
using strange.extensions.command.impl;

namespace ScienceAlert.Core
{
    /// Rather than burden any consumers with implementation details like multiple builders existing
    /// (for example, the default and eva experiment sensor), they can all be wrapped into one master
    /// object that hides all that complexity
    /// 
    // ReSharper disable once ClassNeverInstantiated.Global
    class CommandCreateBuilderComposite<TInterfaceOfCompositeAndBuilders, TComposite> : Command
        where TComposite : TInterfaceOfCompositeAndBuilders
        where TInterfaceOfCompositeAndBuilders : class
    {
        private readonly ITemporaryBindingFactory _tempBindingFactory;
        private readonly ICriticalShutdownEvent _failSignal;

        private class PriorityComparer : IComparer<TInterfaceOfCompositeAndBuilders>
        {
            public int Compare(TInterfaceOfCompositeAndBuilders x, TInterfaceOfCompositeAndBuilders y)
            {
                var first = x.GetAttribute<RegisterBuilderAttribute>();
                var second = y.GetAttribute<RegisterBuilderAttribute>();

                if (first.Any() && second.Any())
                    return first.Value.Priority.CompareTo(second.Value.Priority);
                if (first.Any())
                    return -1;
                return 1;
            }
        }


        public CommandCreateBuilderComposite(
            [NotNull] ITemporaryBindingFactory tempBindingFactory,
            [NotNull] ICriticalShutdownEvent failSignal)
        {
            if (tempBindingFactory == null) throw new ArgumentNullException("tempBindingFactory");
            if (failSignal == null) throw new ArgumentNullException("failSignal");
            if (!GetCompositeConstructor().Any())
                throw new InvalidOperationException("Composite must implement a constructor that accepts an array of " +
                                                    typeof (TInterfaceOfCompositeAndBuilders).Name);
            _tempBindingFactory = tempBindingFactory;
            _failSignal = failSignal;
        }


        public override void Execute()
        {
            var builders = new List<TInterfaceOfCompositeAndBuilders>();

            foreach (var builder in GetRegisteredBuilderTypesMatchingInterface())
            {
                try
                {
                    if (builder.IsAbstract)
                    {
                        Log.Error("Builder " + builder.Name + " is abstract");
                        continue;
                    }

                    if (builder.ContainsGenericParameters)
                    {
                        Log.Error("Builder " + builder.Name + " is an open generic type");
                        continue;
                    }

                    if (!typeof (TInterfaceOfCompositeAndBuilders).IsAssignableFrom(builder))
                    {
                        Log.Error("Registered builder '" + builder.Name + "' does not implement the correct " +
                                  typeof (TInterfaceOfCompositeAndBuilders).Name + " interface!");
                        continue;
                    }

                    using (var binding = _tempBindingFactory.Create(builder))
                    {
                        builders.Add((TInterfaceOfCompositeAndBuilders)binding.GetInstance());
                        Log.Verbose("Added custom builder: " + binding.Key.Name);
                    }
                }
                catch (Exception e)
                {
                    Log.Error("Failed to create builder " + builder.Name + ": " + e);
                }
            }

            builders.Sort(new PriorityComparer());

            Log.Verbose("Listing builders of " + typeof (TInterfaceOfCompositeAndBuilders).Name +
                        " in order of priority");
            foreach (var b in builders)
                Log.Verbose("Builder: " + b.GetType().Name);

            try
            {
                var constructor = GetCompositeConstructor().Value; // guaranteed

                var composite = constructor.Invoke(new object[] {builders.ToArray()});

                if (composite == null)
                    throw new CouldNotCreateInstanceException(typeof (TComposite));

                injectionBinder.Bind<TInterfaceOfCompositeAndBuilders>()
                    .To(composite)
                    .CrossContext();
            }
            catch (Exception e)
            {
                Log.Error("Failed to create composite of " + typeof (TComposite).Name + ": " + e);
                _failSignal.Dispatch();
                Fail();
            }
        }


        private static IEnumerable<Type> GetRegisteredBuilderTypesMatchingInterface()
        {
            return AssemblyLoader.loadedAssemblies
                .SelectMany(la => la.assembly.GetTypes())
                .Where(type => TypeHasRegisterBuilderAttributeMatchingInterface(type));
        }

        private static bool TypeHasRegisterBuilderAttributeMatchingInterface(Type type)
        {
            var attr = type.GetAttribute<RegisterBuilderAttribute>();

            return attr.Any() && attr.Value.BuilderInterface == typeof (TInterfaceOfCompositeAndBuilders);
        }

        private static Maybe<ConstructorInfo> GetCompositeConstructor()
        {
            return typeof (TComposite).GetConstructor(BindingFlags.Instance | BindingFlags.Public, null,
                new[] {typeof (TInterfaceOfCompositeAndBuilders[])}, null).ToMaybe();
        }
    }
}
