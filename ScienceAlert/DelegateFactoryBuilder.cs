using System;
using System.Linq;
using ReeperCommon.Extensions;

namespace ScienceAlert
{
    class DelegateFactoryBuilder<TReturnType, TParam1, TParam2> : IConfigNodeObjectBuilder<TReturnType, TParam1, TParam2>
    {
        private readonly FactoryDelegate _factoryFunc;

        public delegate TReturnType FactoryDelegate(ConfigNode config, TParam1 param1, TParam2 param2);

        private readonly string[] _handledConfigNames;

        public DelegateFactoryBuilder(FactoryDelegate factoryFunc,
            params string[] handledConfigNames)
        {
            if (factoryFunc == null) throw new ArgumentNullException("factoryFunc");
            _factoryFunc = factoryFunc;

            if (handledConfigNames == null) throw new ArgumentNullException("handledConfigNames");

            _handledConfigNames = handledConfigNames.Select(c => c.ToUpperInvariant()).ToArray();
        }


        public TReturnType Build(ConfigNode config, TParam1 parameter, TParam2 parameter2)
        {
            if (!CanHandle(config))
                throw new ArgumentException("This builder cannot handle " + config.ToSafeString());

            // ReSharper disable once EventExceptionNotDocumented
            return _factoryFunc(config, parameter, parameter2);
        }


        public bool CanHandle(ConfigNode config)
        {
            if (config == null) return false;

            var configName = config.name.ToUpperInvariant();

            return _handledConfigNames.Any(handledName => handledName == configName);
        }
    }
}
