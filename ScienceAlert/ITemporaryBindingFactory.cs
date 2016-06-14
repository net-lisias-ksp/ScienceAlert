using System;
using ReeperCommon.Containers;
using strange.extensions.injector.api;

namespace ScienceAlert
{
    public interface ITemporaryBinding : IDisposable
    {
        Type Key { get; }
        object Value { get; }
        Maybe<object> Name { get; }
        object GetInstance();
    }

    public interface ITemporaryBindingFactory
    {
        ITemporaryBinding Create(IInjectionBinder binder, Type type);
        ITemporaryBinding Create(IInjectionBinder binder, Type key, object value);
        ITemporaryBinding Create(IInjectionBinder binder, Type key, object value, object name);
        
        ITemporaryBinding Create(Type type);
        ITemporaryBinding Create(Type key, object value);
        ITemporaryBinding Create(Type key, object value, object name);

        bool CanCreate(Type type);
    }
}