using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using ReeperCommon.Containers;
using ReeperCommon.Logging;
using strange.extensions.command.impl;

namespace ScienceAlert
{
    abstract class CommandConfigureObjectFromConfigNodeBuilders<TBuilderMarkerInterface, TBuiltType> : Command
    {
        protected readonly ITemporaryBindingFactory TemporaryBinder;
        protected readonly Lazy<IEnumerable<Type>> AllTypesInLoadedAssemblies;

        protected CommandConfigureObjectFromConfigNodeBuilders(
            ITemporaryBindingFactory temporaryBinder)
        {
            if (temporaryBinder == null) throw new ArgumentNullException("temporaryBinder");
            TemporaryBinder = temporaryBinder;
            AllTypesInLoadedAssemblies = new Lazy<IEnumerable<Type>>(GetAllTypesFromLoadedAssemblies);
        }

        public sealed override void Execute()
        {
            var builderTypes = GetBuilderTypes();

            if (!builderTypes.Any())
                Log.Warning("No builder types that implement " + typeof (TBuilderMarkerInterface) +
                            " found. This is probably bad");

            BindBuildersToCrossContext(CreateBuilders(builderTypes));
        }


        protected virtual void BindBuildersToCrossContext([NotNull] ReadOnlyCollection<TBuilderMarkerInterface> builders)
        {
            injectionBinder.Bind<ReadOnlyCollection<TBuilderMarkerInterface>>().To(builders).CrossContext();
        }


        protected virtual ReadOnlyCollection<Type> GetBuilderTypes()
        {
            return GetConcreteBuilders();
        }

  

        // actually construct the builder instances
        protected virtual ReadOnlyCollection<TBuilderMarkerInterface> CreateBuilders(ReadOnlyCollection<Type> builderTypes)
        {
            var builderInstances = new List<TBuilderMarkerInterface>();

            foreach (var bt in builderTypes)
            {
                try
                {
                    builderInstances.Add(CreateBuilder(bt));
                }
                catch (Exception e)
                {
                    Log.Error("Failed to create builder: " + bt.FullName);
                    Log.Error("Failure exception: " + e);
                }
            }

            return builderInstances.AsReadOnly();
        }


        /// <exception cref="CouldNotCreateInstanceException">If the temporary binder returns a null instance.</exception>
        /// <exception cref="DependenciesCannotBeSatisfiedException">Temporary binder can't satisfy dependencies.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="builderType"/> is <see langword="null" />.</exception>
        /// <exception cref="ArgumentException"><paramref name="builderType"/> must be an instance of TBuilderMarkerInterface.</exception>
        protected virtual TBuilderMarkerInterface CreateBuilder([NotNull] Type builderType)
        {
            if (builderType == null) throw new ArgumentNullException("builderType");

            if (!typeof (TBuilderMarkerInterface).IsAssignableFrom(builderType))
                throw new ArgumentException(builderType.FullName + " is not assignable to " +
                                            typeof (TBuilderMarkerInterface).Name);


            using (var binding = TemporaryBinder.Create(builderType))
            {
                if (!TemporaryBinder.CanCreate(builderType))
                    throw new DependenciesCannotBeSatisfiedException(builderType);

                var constructed = (TBuilderMarkerInterface) binding.GetInstance();

                if (constructed == null)
                    throw new CouldNotCreateInstanceException(builderType);

                return constructed;
            }
        }

        protected IEnumerable<Type> GetAllTypesFromLoadedAssemblies()
        {
            return AssemblyLoader.loadedAssemblies
                .SelectMany(la => la.assembly.GetTypes());
        }

        protected ReadOnlyCollection<Type> GetConcreteBuilders()
        {
            return GetAllTypesThatImplement<TBuilderMarkerInterface>().ToList().Where(IsConstructable).ToList().AsReadOnly();
        }


        protected ReadOnlyCollection<Type> GetAllTypesThatImplement<TTypeImplemented>()
        {
            return AllTypesInLoadedAssemblies.Value
                .Where(t => !t.IsGenericTypeDefinition && !t.IsAbstract)
                .Where(t => HasMatchingMarkerInterface(t, typeof (TTypeImplemented)))
                .Where(IsNotExcluded)
                .ToList()
                .AsReadOnly();
        }


        protected static bool HasMatchingMarkerInterface(Type possibleBuilderType, Type markerInterface)
        {
            return possibleBuilderType != null && markerInterface.IsAssignableFrom(possibleBuilderType);
        }


        protected static bool IsConstructable(Type type)
        {
            return type != null && type.GetConstructors(BindingFlags.Instance | BindingFlags.Public).Any();
        }


        protected static bool IsNotExcluded(Type type)
        {
            return type != null &&
                   !type.GetCustomAttributes(typeof(DoNotAutoRegister), false).Any();
        }
    }
}