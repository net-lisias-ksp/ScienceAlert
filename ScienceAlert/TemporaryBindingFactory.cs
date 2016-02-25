using System;
using System.Linq;
using System.Reflection;
using ReeperCommon.Logging;
using strange.extensions.injector.api;

namespace ScienceAlert
{
// ReSharper disable once ClassNeverInstantiated.Global
    public class TemporaryBindingFactory : ITemporaryBindingFactory
    {
        private readonly IInjectionBinder _binder;

        public TemporaryBindingFactory(IInjectionBinder binder)
        {
            if (binder == null) throw new ArgumentNullException("binder");
            _binder = binder;
        }


        public ITemporaryBinding Create(Type type)
        {
            return Create(_binder, type);
        }


        public ITemporaryBinding Create(IInjectionBinder binder, Type type)
        {
            if (binder == null) throw new ArgumentNullException("binder");
            if (type == null) throw new ArgumentNullException("type");

            bool alreadyHasBinding = binder.GetBinding(type) != null;

            if (!alreadyHasBinding)
                binder.Bind(type).To(type);

            return new TemporaryBinding(binder, type, !alreadyHasBinding);
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