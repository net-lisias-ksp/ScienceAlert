using System;
using System.Linq;
using System.Reflection;
using strange.extensions.injector.api;

namespace ScienceAlert
{
    public class TemporaryBindingInstanceFactory : ITemporaryBindingInstanceFactory
    {
        private readonly IInjectionBinder _binder;

        public TemporaryBindingInstanceFactory(IInjectionBinder binder)
        {
            if (binder == null) throw new ArgumentNullException("binder");
            _binder = binder;
        }


        public object Create(Type concreteType)
        {
            return Create(_binder, concreteType);
        }


        public object Create(IInjectionBinder binder, Type concreteType)
        {
            if (binder == null) throw new ArgumentNullException("binder");
            if (concreteType == null) throw new ArgumentNullException("concreteType");
            if (concreteType.IsGenericTypeDefinition)
                throw new ArgumentException(concreteType.FullName + " is a generic type definition", "concreteType");
            if (concreteType.IsAbstract)
                throw new ArgumentException(concreteType.FullName + " is abstract and cannot be created", "concreteType");
            if (concreteType.IsInterface)
                throw new ArgumentException(concreteType.FullName + " is an interface and cannot be created",
                    "concreteType");

            bool hasBinding = binder.GetBinding(concreteType) != null;

            try
            {
                if (!hasBinding) binder.Bind(concreteType).To(concreteType);

                return binder.GetInstance(concreteType);
            }
            finally
            {
                if (!hasBinding) binder.Unbind(concreteType);
            }
        }


        public bool CanCreate(Type concreteType)
        {
            if (concreteType == null) throw new ArgumentNullException("concreteType");
            if (concreteType.IsAbstract)
                throw new ArgumentException(concreteType.FullName + " is abstract and cannot be created", "concreteType");

            var constructors = concreteType.GetConstructors(BindingFlags.Public | BindingFlags.Instance);
            var binding = _binder.GetBinding(concreteType);

            Log.Verbose("Checking if CanCreate " + concreteType.Name);

            var canCreate = binding != null &&
                       constructors.Any(
                           ci => ci.GetParameters().Select(pi => pi.ParameterType).All(CanCreate));

            Log.Verbose("result: " + canCreate);

            return canCreate;
        }
    }
}
