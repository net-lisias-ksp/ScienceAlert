using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using ReeperCommon.Serialization;

namespace ScienceAlert.Core
{
    public class GetObjectFieldsIncludingBaseTypes : IGetObjectFields
    {
        public IEnumerable<FieldInfo> Get(object target)
        {
            var targetType = target.GetType();
            var targetTypes = new List<Type> { targetType };


            while (targetType.BaseType != null)
            {
                targetTypes.Add(targetType.BaseType);
                targetType = targetType.BaseType;
            }

            return targetTypes.SelectMany(GetSerializableFields);
        }


        private static IEnumerable<FieldInfo> GetSerializableFields(Type target)
        {
            return target.GetFields(BindingFlags.Instance | BindingFlags.DeclaredOnly | BindingFlags.Public |
                             BindingFlags.NonPublic)
                .Where(fi => fi.DeclaringType == target)
                .Where(fi => fi.GetCustomAttributes(true).Any(attr => attr is ReeperPersistentAttribute));
        }
    }
}
