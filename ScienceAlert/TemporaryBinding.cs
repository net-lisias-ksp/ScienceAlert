using System;
using System.Linq;
using JetBrains.Annotations;
using ReeperCommon.Containers;
using strange.extensions.injector.api;
using strange.extensions.injector.impl;

namespace ScienceAlert
{
    class TemporaryBinding : ITemporaryBinding
    {
        private readonly IInjectionBinder _binder;
        private readonly bool _mustUnbind;

        public Type Key { get; private set; }
        public object Value { get; private set; }
        public Maybe<object> Name { get; private set; }


        public TemporaryBinding(IInjectionBinder binder, Type binding, bool mustUnbind)
            : this(binder, binding, binding, mustUnbind)
        {

        }

        public TemporaryBinding(IInjectionBinder binder, Type key, object binding, bool mustUnbind)
            : this(binder, key, binding, Maybe<object>.None, mustUnbind)
        {

        }

        public TemporaryBinding([NotNull] IInjectionBinder binder, [NotNull] Type key, [CanBeNull] object binding, Maybe<object> name, bool mustUnbind)
        {
            if (binder == null) throw new ArgumentNullException("binder");
            if (key == null) throw new ArgumentNullException("key");
            if (binding == null) throw new ArgumentNullException("binding");

 
            _binder = binder;
            _mustUnbind = mustUnbind;
            Key = key;
            Value = binding;
            Name = name;
        }


        private void Dispose(bool disposing)
        {
            if (!disposing) return;

            if (_mustUnbind)
            {
                if (!Name.Any())
                    _binder.Unbind(Key);
                else _binder.Unbind(Key, Name.Value);
            }
            GC.SuppressFinalize(this);
        }


        public void Dispose()
        {
            Dispose(true);
        }


        public object GetInstance()
        {
            try
            {
                if (!Name.Any())
                    return _binder.GetInstance(Key);
                return _binder.GetInstance(Key, Name.Value);
            }
            catch (InjectionException ie)
            {
                throw new ArgumentException("Exception while injecting temporary binding of " + Key.Name, ie);
            }
        }
    }
}