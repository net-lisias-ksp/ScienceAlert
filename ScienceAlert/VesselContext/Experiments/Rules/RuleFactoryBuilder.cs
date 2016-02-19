using System;
using ReeperCommon.Serialization;
using strange.extensions.injector.api;

namespace ScienceAlert.VesselContext.Experiments.Rules
{
    public class RuleFactoryBuilder<TRuleType> : IConfigNodeObjectGraphBuilder<IRuleFactory>
    {
        private readonly ITemporaryBindingFactory _temporaryBinder;

        class RuleFactory : IRuleFactory
        {
            private readonly Type _ruleType;
            private readonly ConfigNode _data;
            private readonly ITemporaryBindingFactory _temporaryBinder;

            public RuleFactory(Type ruleType, ConfigNode data, ITemporaryBindingFactory temporaryBinder)
            {
                if (ruleType == null) throw new ArgumentNullException("ruleType");
                if (data == null) throw new ArgumentNullException("data");
                if (temporaryBinder == null) throw new ArgumentNullException("temporaryBinder");
                if (ruleType.IsGenericTypeDefinition || ruleType.IsAbstract || ruleType.IsInterface)
                    throw new ArgumentException(ruleType.FullName + " is not a concrete type", "ruleType");

                _ruleType = ruleType;
                _data = data;
                _temporaryBinder = temporaryBinder;
            }


            public IExperimentRule Create(IInjectionBinder context, IConfigNodeSerializer serializer)
            {
                if (context == null) throw new ArgumentNullException("context");
                if (serializer == null) throw new ArgumentNullException("serializer");

             
                using (var binding = _temporaryBinder.Create(context, _ruleType))
                {
                    var instance = (IExperimentRule)binding.GetInstance();

                    serializer.LoadObjectFromConfigNode(ref instance, _data);

                    return instance;
                }
            }
        }


        public RuleFactoryBuilder(ITemporaryBindingFactory temporaryBinder)
        {
            if (temporaryBinder == null) throw new ArgumentNullException("temporaryBinder");
            _temporaryBinder = temporaryBinder;
        }


        private static string GetTypeNameFromConfig(ConfigNode config)
        {
            if (config == null) throw new ArgumentNullException("config");

            return config.name;
        }


        public IRuleFactory Build(IConfigNodeObjectGraphBuilder<IRuleFactory> builder, ConfigNode config)
        {
            if (config == null) throw new ArgumentNullException("config");

            if (!CanHandle(config))
                throw new ArgumentException("This builder cannot handle the specified ConfigNode", "config");

            return new RuleFactory(typeof (TRuleType), config, _temporaryBinder);
        }


        public IRuleFactory Build(ConfigNode config)
        {
            return Build(null, config);
        }


        public bool CanHandle(ConfigNode config)
        {
            var typeNameInConfig = GetTypeNameFromConfig(config).ToUpperInvariant();

            return !string.IsNullOrEmpty(typeNameInConfig) && (
                typeNameInConfig == typeof (TRuleType).FullName.ToUpperInvariant() ||
                typeNameInConfig == typeof (TRuleType).Name.ToUpperInvariant());
        }


        public override string ToString()
        {
            return typeof (RuleFactoryBuilder<TRuleType>).Name + "[ " + typeof (TRuleType).Name + "]";
        }
    }
}
