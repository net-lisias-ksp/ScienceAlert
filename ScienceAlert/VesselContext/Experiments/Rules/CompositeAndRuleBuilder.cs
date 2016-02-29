using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ScienceAlert.VesselContext.Experiments.Rules
{
    public class CompositeAndRuleBuilder : IRuleBuilder
    {
        private readonly string[] _validNodeNames = {"ALL", "ALL_OF", "AND", "EVERYTHING"};


        public IExperimentRule Build(ConfigNode config, IRuleBuilderProvider parameter, ITemporaryBindingFactory bindingFactory)
        {
            if (config == null) throw new ArgumentNullException("config");
            if (parameter == null) throw new ArgumentNullException("parameter");
            if (!CanHandle(config)) throw new ArgumentException(this + " cannot handle " + config);

            return Create(CreateSubrules(config, parameter, bindingFactory));
        }


        protected virtual IExperimentRule Create(IEnumerable<IExperimentRule> subRules)
        {
            return new CompositeAndRule(subRules);
        }


        protected static IEnumerable<IExperimentRule> CreateSubrules(ConfigNode config, IRuleBuilderProvider builders, ITemporaryBindingFactory bindingFactory)
        {
            if (config == null) throw new ArgumentNullException("config");

            return config.nodes.Cast<ConfigNode>().Select(c =>
            {
                var builder = builders.GetBuilder(c);

                if (!builder.Any())
                    throw new ArgumentException("No builder found capable of handling " + c);

                return builder.Value.Build(c, builders, bindingFactory);
            });
        }


        public bool CanHandle(ConfigNode config)
        {
            if (config == null) return false;

            return _validNodeNames.Select(nn => nn.ToUpperInvariant()).Any(nn => nn.Equals(config.name.ToUpperInvariant()));
        }
    }
}
