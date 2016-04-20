using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using ReeperCommon.Containers;

namespace ScienceAlert
{

    class ObjectFromConfigNodeBuilderTypeQueries
    {
        private readonly Lazy<IEnumerable<Type>> _allTypesInLoadedAssemblies;

        public ObjectFromConfigNodeBuilderTypeQueries()
        {
            _allTypesInLoadedAssemblies = new Lazy<IEnumerable<Type>>(GetAllTypesFromLoadedAssemblies);
        }


        public ReadOnlyCollection<Type> GetConcreteBuilders<TBuilderMarkerType>()
        {
            return _allTypesInLoadedAssemblies.Value
                .Where(t => !t.IsGenericTypeDefinition)
                .Where(IsConstructable)
                .Where(HasMatchingMarkerInterface<TBuilderMarkerType>)
                .Where(IsNotExcluded)
                .ToList()
                .AsReadOnly();
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


        private static bool IsConstructable(Type type)
        {
            return type != null && type.GetConstructors(BindingFlags.Instance | BindingFlags.Public).Any();
        }


        private static bool IsNotExcluded(Type type)
        {
            return type != null &&
                   !type.GetCustomAttributes(typeof(DoNotAutoRegister), false).Any();
        }
    }
}