using System;
using System.Linq;
using ReeperCommon.Containers;
using ReeperCommon.Serialization;
using strange.extensions.injector.api;
using strange.framework.impl;

namespace ScienceAlert.VesselContext.Experiments.Rules
{
// ReSharper disable once ClassNeverInstantiated.Global
    public class ExperimentRuleFactory : IExperimentRuleFactory
    {
        private readonly IInjectionBinder _binder;
        private readonly IConfigNodeSerializer _serializer;

        public ExperimentRuleFactory(IInjectionBinder binder, IConfigNodeSerializer serializer)
        {
            if (binder == null) throw new ArgumentNullException("binder");
            if (serializer == null) throw new ArgumentNullException("serializer");
            _binder = binder;
            _serializer = serializer;
        }


        public IExperimentRule Create(ScienceExperiment experiment, RuleDefinition definition)
        {
            if (experiment == null) throw new ArgumentNullException("experiment");
            if (definition == null) throw new ArgumentNullException("definition");

            switch (definition.Type)
            {
                case RuleDefinition.DefinitionType.CompositeAll:
                    return CreateCompositeAllRule(experiment, definition);

                case RuleDefinition.DefinitionType.Rule:
                    return CreateRule(experiment, definition);

                default:
                    throw new NotImplementedException("Unrecognized rule type: " + definition.Type);
            }
        }


        private IExperimentRule CreateCompositeAllRule(ScienceExperiment experiment, RuleDefinition definition)
        {
            return new CompositeAllRule(definition.Rules.Select(ruleDefinition => Create(experiment, ruleDefinition)).ToArray());
        }


        private IExperimentRule CreateRule(ScienceExperiment experiment, RuleDefinition definition)
        {
            if (!definition.Rule.Any())
                throw new ArgumentException("RuleDefinition must define a rule type", "definition");

            try
            {
                _binder.Bind<ScienceExperiment>().To(experiment);//.CrossContext();

                Log.Debug(() => "Creating experiment rule from rule definition: " + definition.Rule.Value.FullName);

                var rule = _binder.GetInstance(definition.Rule.Value) as IExperimentRule;


                if (rule == null || !rule.GetType().IsAssignableFrom(definition.Rule.Value))
                    throw new FailedToCreateRuleException(definition);

                if (definition.RuleConfig.Any() && definition.RuleConfig.Value.HasData)
                    _serializer.LoadObjectFromConfigNode(ref rule, definition.RuleConfig.Value);

                return rule;
            }
            catch (BinderException)
            {
                Log.Error("Can't create " + definition.Rule.Return(rt => rt.FullName, "<no rule type specified>") + " rule for " + experiment.id + " due to binder exception!");
                throw;
            }
            finally
            {
                _binder.Unbind<ScienceExperiment>();
            }
        }
    }
}
