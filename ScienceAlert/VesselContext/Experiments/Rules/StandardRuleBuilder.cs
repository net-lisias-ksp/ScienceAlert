using System;
using System.Linq;
using ReeperCommon.Containers;
using ReeperCommon.Serialization;

namespace ScienceAlert.VesselContext.Experiments.Rules
{
    // This builder is very simple: given a ConfigNode with a name that matches the Type name,
    // create an instance (using a temporary binding factory) and deserialize the object from that config
    public class StandardRuleBuilder<TRuleType> : IRuleBuilder where TRuleType : IExperimentRule
    {
        private readonly IConfigNodeSerializer _serializer;

        public StandardRuleBuilder(IConfigNodeSerializer serializer)
        {
            if (serializer == null) throw new ArgumentNullException("serializer");
            if (typeof (TRuleType).IsAbstract || typeof (TRuleType).IsInterface)
                throw new InvalidOperationException("The type " + typeof (TRuleType) + " is not a valid rule");

            _serializer = serializer;
        }


        public override string ToString()
        {
            return typeof(StandardRuleBuilder<TRuleType>).Name + "[ " + typeof(TRuleType).Name + "]";
        }


        public IExperimentRule Build(ConfigNode config, IRuleBuilderProvider builders, ITemporaryBindingFactory bindingFactory)
        {
            if (config == null) throw new ArgumentNullException("config");
            if (bindingFactory == null) throw new ArgumentNullException("bindingFactory");

            if (!CanHandle(config)) throw new ArgumentException(this + " cannot handle " + config);

            using (var binding = bindingFactory.Create(typeof(TRuleType)))
            {
                var instance = (TRuleType)binding.GetInstance();

                _serializer.LoadObjectFromConfigNode(ref instance, config);

                return instance;
            }
        }


        public bool CanHandle(ConfigNode config)
        {
            if (config == null) return false;

            return typeof (TRuleType).With(rt => new[] {rt.FullName.ToUpperInvariant(), rt.Name.ToUpperInvariant()})
                .Any(typeName => typeName.Equals(config.name.ToUpperInvariant()));
        }
    }
}
