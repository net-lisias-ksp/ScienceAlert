using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using ReeperCommon.Containers;
using ReeperCommon.Logging;
using strange.extensions.command.impl;

namespace ScienceAlert
{
    class ObjectFromConfigNodeBuilderTypeQueries
    {
        private readonly Lazy<IEnumerable<Type>> _allTypesInLoadedAssemblies;

        public ObjectFromConfigNodeBuilderTypeQueries()
        {
            _allTypesInLoadedAssemblies = new Lazy<IEnumerable<Type>>(GetAllTypesFromLoadedAssemblies);
        }


        public IEnumerable<Type> GetAllBuilders<TTypeProduced, TBuilderMarkerType>()
        {
            return _allTypesInLoadedAssemblies.Value
                // ReSharper disable once HeapView.SlowDelegateCreation
                .Where(HasMatchingMarkerInterface<TBuilderMarkerType>)
                .Where(IsNotExcluded);
        }

        public IEnumerable<Type> GetGenericBuilders<TTypeProduced, TBuilderMarkerType>()
        {
            return null;
        }
        
        public IEnumerable<Type> GetConcreteBuilders<TTypeProduced, TBuilderMarkerType>()
        {
            return GetAllBuilders<TBuilderMarkerType>()
                .Where(IsConcreteInstanceOf
                .Where(IsConstructable);
            //var specificBuilderTypes = _allTypesInLoadedAssemblies
            //    .Where(IsBuilderThatMatchesInterfaceAndProducedType)
            //    .Where(IsConstructable)
            //    .Where(IsNotExcluded)
            //    .ToList();
        }


        private static IEnumerable<Type> GetAllTypesFromLoadedAssemblies()
        {
            return AssemblyLoader.loadedAssemblies
                .SelectMany(la => la.assembly.GetTypes());
        }


        private static bool HasMatchingMarkerInterface<TBuilderMarkerInterface>(Type possibleBuilderType)
        {
            return possibleBuilderType != null && !possibleBuilderType.IsAbstract && !possibleBuilderType.IsGenericTypeDefinition &&
                   typeof(TBuilderMarkerInterface).IsAssignableFrom(possibleBuilderType);
        }


        private static bool IsConcreteInstanceOf(Type baseType, Type queryType)
        {
            return !queryType.IsAbstract && !queryType.IsGenericTypeDefinition &&
                   baseType.IsAssignableFrom(queryType) && IsConstructable(queryType);
        }

        private static bool IsConstructable(Type type)
        {
            return type != null && type.GetConstructors(BindingFlags.Instance | BindingFlags.Public).Any();
        }


        protected virtual bool IsNotExcluded(Type type)
        {
            return type != null &&
                   !type.GetCustomAttributes(typeof(ExcludeFromConventionalRegistrationAttribute), false).Any();
        }
    }


    abstract class CommandConfigureObjectFromConfigNodeBuilders<TTypeProduced, TBuilderMarkerInterface> : Command
    {
        protected readonly ITemporaryBindingFactory TemporaryBinder;
        protected readonly Lazy<IEnumerable<Type>> AllTypesInLoadedAssemblies;
  
        protected CommandConfigureObjectFromConfigNodeBuilders(ITemporaryBindingFactory temporaryBinder)
        {
            if (temporaryBinder == null) throw new ArgumentNullException("temporaryBinder");
            TemporaryBinder = temporaryBinder;
            AllTypesInLoadedAssemblies = new Lazy<IEnumerable<Type>>(GetAllTypesFromLoadedAssemblies);
        }

        public override void Execute()
        {

        }



        protected virtual IEnumerable<TBuilderMarkerInterface> CreateBuilders()
        {

        }











        //private IEnumerable<Type> GetAllConcreteTypesAssignableFrom<T>()
        //{
        //    // ReSharper disable once HeapView.SlowDelegateCreation
        //    return AllTypesInLoadedAssemblies.Value.Where(t => IsConcreteInstanceOf(typeof(T), t)).Where(IsNotExcluded);
        //}

   


        private TBuilderMarkerInterface CreateBuilder(Type builderType)
        {
            if (builderType == null) throw new ArgumentNullException("builderType");
            if (builderType.IsAbstract || builderType.IsGenericTypeDefinition)
                throw new ArgumentException("Cannot create " + builderType);

            if (!typeof(TBuilderMarkerInterface).IsAssignableFrom(builderType))
                throw new ArgumentException(builderType + " does not implement " + typeof(TBuilderMarkerInterface).Name);

            using (var builderBinding = TemporaryBinder.Create(builderType))
            {
                var returnValue = (TBuilderMarkerInterface)builderBinding.GetInstance();

                if (returnValue == null)
                    throw new ArgumentException("Unable to create " + builderType);

                return returnValue;
            }
        }

        //protected virtual IEnumerable<Type> GetConcreteBuilderTypes<TTypeThatMarksBuilders, TTypeThatMarksThingsThatAreBuilt>(
        //    Type genericBuilderTypeDefinition)
        //{
        //    if (!genericBuilderTypeDefinition.IsGenericTypeDefinition)
        //        throw new ArgumentException(genericBuilderTypeDefinition.Name + " must be the generic version");

        //    var concreteBuilderTypes = GetAllConcreteTypesAssignableFrom<TTypeThatMarksBuilders>().Where(IsNotExcluded).ToList();
        //    var allTypesThatMightBeBuilt =
        //        GetAllConcreteTypesAssignableFrom<TTypeThatMarksThingsThatAreBuilt>().ToList();

        //    foreach (var t in concreteBuilderTypes)
        //        Log.Debug("Found concrete builder: " + t);

        //    foreach (var t in allTypesThatMightBeBuilt)
        //        Log.Debug("A generic builder will be created for: " + t);

        //    // create a generic builder for each rule type. These are very nonspecial builders that simply use
        //    // the IoC container to try to satisfy the constructor of the rule with no extra knowledge of any of
        //    // the rule's requirements. More complex rules can have their own special builder that will be preferred
        //    // over these
        //    var genericBuilders =
        //        allTypesThatMightBeBuilt
        //            // ReSharper disable once HeapView.DelegateAllocation
        //            // ReSharper disable once HeapView.SlowDelegateCreation
        //            .Select(t => genericBuilderTypeDefinition.MakeGenericType(t));

        //    return concreteBuilderTypes
        //        .Union(genericBuilders);
        //}






        //protected IEnumerable<TBuilderInterface> CreateBuilderInstances<TBuilderInterface, TThingBuilderBuilders>(
        //    Type genericBuilderTypeDefinition)
        //{
        //    var builderInstances = new List<TBuilderInterface>();
        //    var builderTypes =
        //        GetConcreteBuilderTypes<TBuilderInterface, TThingBuilderBuilders>(genericBuilderTypeDefinition)
        //            .ToList();

        //    // bind the types first in case a builder has a dependency on another builder
        //    builderTypes.ForEach(builderType => injectionBinder.Bind(builderType).CrossContext());

        //    foreach (var ty in builderTypes)
        //    {
        //        try
        //        {
        //            var instance = CreateBuilder<TBuilderInterface>(ty);

        //            builderInstances.Add(instance);
        //        }
        //        catch (Exception e)
        //        {
        //            Log.Warning("Could not create " + typeof(TBuilderInterface).Name + " of type " + ty.FullName +
        //                        " due to: " + e);
        //        }
        //    }

        //    return builderInstances;
        //}
    }
}