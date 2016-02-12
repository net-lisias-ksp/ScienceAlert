using System;
using strange.extensions.injector.api;

namespace ScienceAlert
{
    public interface ITemporaryBindingInstanceFactory
    {
        object Create(Type concreteType);
        object Create(IInjectionBinder binder, Type concreteType);
        bool CanCreate(Type concreteType);
    }
}
