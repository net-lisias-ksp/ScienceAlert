using System;
using strange.extensions.injector.api;

namespace ScienceAlert
{
    public interface ITemporaryBinding : IDisposable
    {
        Type BoundType { get; }
        object GetInstance();
    }

    public interface ITemporaryBindingFactory
    {
        ITemporaryBinding Create(IInjectionBinder binder, Type type);
        ITemporaryBinding Create(Type type);
        bool CanCreate(Type type);
    }
}