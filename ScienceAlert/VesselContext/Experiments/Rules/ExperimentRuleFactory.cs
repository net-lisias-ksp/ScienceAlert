using System;
using System.Collections.Generic;
using System.Linq;
using ReeperCommon.Containers;
using ReeperCommon.Serialization;
using ScienceAlert.Core;
using strange.extensions.injector.api;

namespace ScienceAlert.VesselContext.Experiments.Rules
{
    public class ExperimentRuleFactory : IExperimentRuleFactory
    {
        private readonly IInjectionBinder _injectionBinder;
        private readonly IConfigNodeSerializer _serializer;
        private readonly List<Type> _ruleTypes;  

        public ExperimentRuleFactory([Name(CoreKeys.ExperimentRuleTypes)] IEnumerable<Type> experimentRuleTypes, IInjectionBinder injectionBinder, IConfigNodeSerializer serializer)
        {
            if (experimentRuleTypes == null) throw new ArgumentNullException("experimentRuleTypes");
            if (injectionBinder == null) throw new ArgumentNullException("injectionBinder");
            if (serializer == null) throw new ArgumentNullException("serializer");

            _injectionBinder = injectionBinder;
            _serializer = serializer;

            _ruleTypes = experimentRuleTypes.ToList();
        }

        
        private IExperimentRule CreateCompositeRule(
            ScienceExperiment experiment, 
            CompositeRule.CompositeType compositeType, 
            ConfigNode config)
        {
            if (experiment == null) throw new ArgumentNullException("experiment");
            if (config == null) throw new ArgumentNullException("config");
            if (!config.HasData || config.CountNodes == 0)
                throw new CompositeRuleEmptyException(experiment.id + " contains a composite rule with no entries");

            var subrules = config.GetNodes().Select(n => Create(experiment, n)).ToArray();

            return new CompositeRule(compositeType, subrules);
        }


        private IExperimentRule CreateSingleRule(ScienceExperiment experiment, ConfigNode config)
        {
            // if there's more than one node, user probably wanted to AND them together
            if (config.CountNodes > 1)
            {
                Log.Warning("More than one sub node in a rule definition for " + experiment.id +
                            " which exists outside of a composite node -- assuming AND rule");

                return CreateCompositeRule(experiment, CompositeRule.CompositeType.All, config);
            }

            if (config.CountNodes == 0 || !config.HasData) // having no rules is technically acceptable
                return new CompositeRule(CompositeRule.CompositeType.All, new IExperimentRule[0]);


            var nodeContainingRuleData = config.nodes[0];

            var targetRuleName = nodeContainingRuleData.name;

            if (string.IsNullOrEmpty(targetRuleName))
                throw new ArgumentException("A rule ConfigNode for " + experiment.id + " does not supply a rule name",
                    "config");

            var targetRuleType = 
                _ruleTypes.FirstOrDefault(t => t.FullName == targetRuleName).ToMaybe()
                    .Or(_ruleTypes.FirstOrDefault(t => t.Name == targetRuleName).ToMaybe())
                    .Or(Type.GetType(targetRuleName).ToMaybe())
                    .IfNull(() => { throw new RuleTypeNotFoundException(targetRuleName); });


            try
            {
                _injectionBinder.Bind<ScienceExperiment>().ToValue(experiment);

                var rule = (IExperimentRule) _injectionBinder.GetInstance(targetRuleType.Value);

                if (nodeContainingRuleData.HasData)
                    _serializer.LoadObjectFromConfigNode(ref rule, nodeContainingRuleData);

                return rule;
            }
            catch (Exception)
            {
                throw new FailedToCreateRuleException(targetRuleType.Value);
            }
            finally
            {
                _injectionBinder.Unbind<ScienceExperiment>();
            }
        }



        public IExperimentRule Create(ScienceExperiment experiment, ConfigNode config)
        {
            if (experiment == null) throw new ArgumentNullException("experiment");
            if (config == null) throw new ArgumentNullException("config");
            if (string.IsNullOrEmpty(config.name))
                throw new ArgumentException("ConfigNode does not supply a name");

            var ruleNodeTypeOrRuleType = config.name;

            try
            {
                var result =
                    (CompositeRule.CompositeType)
                        Enum.Parse(typeof (CompositeRule.CompositeType), ruleNodeTypeOrRuleType, true);

                return CreateCompositeRule(experiment, result, config);
            }
            catch (ArgumentException)
            {
                // the name doesn't match any of the enum .. but that's okay, it might be a rule name instead
                return CreateSingleRule(experiment, config);
            }
            catch (OverflowException oe)
            {
                Log.Error("Unrecognized node type specifier: " + ruleNodeTypeOrRuleType);
                throw;
            }
        }
    }
}
