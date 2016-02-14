using System;
using System.Collections.Generic;
using System.Linq;
using ReeperCommon.Serialization;
using strange.extensions.injector.api;

namespace ScienceAlert.VesselContext.Experiments.Rules
{
    class CompositeAndRule : IExperimentRule
    {
        private readonly IExperimentRule[] _rules;

        public CompositeAndRule(IEnumerable<IExperimentRule> rules)
        {
            if (rules == null) throw new ArgumentNullException("rules");
            _rules = rules.ToArray();
        }


        public bool Passes()
        {
            return _rules.All(r => r.Passes());
        }


        public class CompositeAndRuleFactoryBuilder : IConfigNodeObjectGraphBuilder<IRuleFactory>
        {
            private readonly string[] _handled = { "ALL", "ALL_OF", "AND" };


            private class CompositeAndRuleFactory : IRuleFactory
            {
                private readonly IRuleFactory[] _subRules;

                public CompositeAndRuleFactory(IEnumerable<IRuleFactory> subRules)
                {
                    if (subRules == null) throw new ArgumentNullException("subRules");
                    _subRules = subRules.ToArray();
                }


                public IExperimentRule Create(IInjectionBinder context, IConfigNodeSerializer serializer)
                {
                    return new CompositeAndRule(_subRules.Select(f => f.Create(context, serializer)));
                }
            }



            private static IEnumerable<ConfigNode> GetFactoryConfigs(ConfigNode @from)
            {
                if (@from == null) throw new ArgumentNullException("from");

                return @from.nodes.Cast<ConfigNode>();
            }


            public IRuleFactory Build(IConfigNodeObjectGraphBuilder<IRuleFactory> builder, ConfigNode config)
            {
                if (builder == null) throw new ArgumentNullException("builder");
                if (config == null) throw new ArgumentNullException("config");

                if (!CanHandle(config))
                    throw new ArgumentException("This builder can't handle " + config.name);

                var subFactories = GetFactoryConfigs(config).Select(subNode => builder.Build(builder, subNode));

                return new CompositeAndRuleFactory(subFactories);
            }


            public IRuleFactory Build(ConfigNode config)
            {
                return Build(this, config);
            }


            public bool CanHandle(ConfigNode config)
            {
                if (config == null) throw new ArgumentNullException("config");

                return _handled.Any(handled => String.Equals(handled, config.name, StringComparison.InvariantCultureIgnoreCase));
            }


            public override string ToString()
            {
                return typeof (CompositeAndRuleFactoryBuilder).Name + " [ " + string.Join(",", _handled) + "]";
            }
        }
    }
}