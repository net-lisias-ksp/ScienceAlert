using System;
using System.Linq;
using ReeperCommon.Extensions;

namespace ScienceAlert
{
    class ObjectFromConfigNodeBuilderUsingFactoryDelegate<TReturnType, TParam1, TParam2, TParam3> : IConfigNodeObjectBuilder<TReturnType, TParam1, TParam2, TParam3>
    {
        private readonly FactoryDelegate _factoryFunc;

        public delegate TReturnType FactoryDelegate(ConfigNode config, TParam1 param1, TParam2 param2, TParam3 param3);

        private readonly string[] _handledConfigNames;

        public ObjectFromConfigNodeBuilderUsingFactoryDelegate(FactoryDelegate factoryFunc,
            params string[] handledConfigNames)
        {
            if (factoryFunc == null) throw new ArgumentNullException("factoryFunc");
            _factoryFunc = factoryFunc;

            if (handledConfigNames == null) throw new ArgumentNullException("handledConfigNames");

            _handledConfigNames = handledConfigNames.Select(c => c.ToUpperInvariant()).ToArray();
        }


        public TReturnType Build(ConfigNode config, TParam1 parameter, TParam2 parameter2, TParam3 parameter3)
        {
            if (!CanHandle(config))
                throw new ArgumentException("This builder cannot handle " + config.ToSafeString());

            // ReSharper disable once EventExceptionNotDocumented
            return _factoryFunc(config, parameter, parameter2, parameter3);
        }


        public bool CanHandle(ConfigNode config)
        {
            if (config == null) return false;

            var configName = config.name.ToUpperInvariant();

            return _handledConfigNames.Any(handledName => handledName == configName);
        }
    }
}
