using System;
using System.Diagnostics;
using System.Linq;
using System.Net;
using JetBrains.Annotations;
using ReeperCommon.Containers;
using ReeperCommon.Logging;
using ReeperKSP.Extensions;
using strange.extensions.injector.api;
using ScienceAlert.Game;
using ScienceAlert.VesselContext.Experiments.Sensors.Rules;
using Debug = UnityEngine.Debug;

namespace ScienceAlert.VesselContext.Experiments.Sensors
{
    [RegisterBuilder(typeof(IObjectFromConfigNodeBuilder<IExperimentSensor, ExperimentConfiguration, IInjectionBinder>), int.MaxValue)]
    public class DefaultExperimentSensorBuilder : IObjectFromConfigNodeBuilder<IExperimentSensor, ExperimentConfiguration, IInjectionBinder>
    {
        private readonly IObjectFromConfigNodeBuilder<ISensorRule, ConfigNode, IInjectionBinder> _ruleBuilder;
        private readonly ConfigNode _defaultOnboardConfig;
        private readonly ConfigNode _defaultAvailabilityConfig;
        private readonly ConfigNode _defaultConditionConfig;
        private readonly ITemporaryBindingFactory _tempBinding;

        private const string ConditionNodeName = "CONDITION";
        private const string AvailabilityNodeName = "AVAILABILITY";
        private const string OnboardNodeName = "ONBOARD";

        private class DefaultScienceSubject : IScienceSubject
        {
            private readonly ScienceSubject _subject;

            public DefaultScienceSubject()
            {
                _subject = new ScienceSubject("not set", "not set", 0f, 0f, 0f);
            }

            public ScienceSubject Subject
            {
                get { return _subject; }
            }

            public string Id
            {
                get { return _subject.id; }
            }

            public float DataScale
            {
                get { return _subject.dataScale; }
            }

            public float Science
            {
                get { return _subject.science; }
            }

            public float ScientificValue
            {
                get { return _subject.scientificValue; }
            }

            public float ScienceCap
            {
                get { return _subject.scienceCap; }
            }

            public float SubjectValue
            {
                get { return _subject.subjectValue; }
            }
        }

        public DefaultExperimentSensorBuilder(
            [NotNull] IObjectFromConfigNodeBuilder<ISensorRule, ConfigNode, IInjectionBinder> ruleBuilder,
            [NotNull, Name(CrossContextKeys.DefaultOnboardRule)] ConfigNode defaultOnboardConfig,
            [NotNull, Name(CrossContextKeys.DefaultAvailabilityRule)] ConfigNode defaultAvailabilityConfig,
            [NotNull, Name(CrossContextKeys.DefaultConditionRule)] ConfigNode defaultConditionConfig,
            [NotNull] ITemporaryBindingFactory tempBinding)
        {
            if (ruleBuilder == null) throw new ArgumentNullException("ruleBuilder");
            if (defaultOnboardConfig == null) throw new ArgumentNullException("defaultOnboardConfig");
            if (defaultAvailabilityConfig == null) throw new ArgumentNullException("defaultAvailabilityConfig");
            if (defaultConditionConfig == null) throw new ArgumentNullException("defaultConditionConfig");
            if (tempBinding == null) throw new ArgumentNullException("tempBinding");

            _ruleBuilder = ruleBuilder;
            _defaultOnboardConfig = defaultOnboardConfig;
            _defaultAvailabilityConfig = defaultAvailabilityConfig;
            _defaultConditionConfig = defaultConditionConfig;
            _tempBinding = tempBinding;
        }


        public IExperimentSensor Build(ExperimentConfiguration config, [NotNull] IInjectionBinder binder, IObjectFromConfigNodeBuilder<IExperimentSensor, ExperimentConfiguration, IInjectionBinder> rootBuilder = null)
        {
            if (binder == null) throw new ArgumentNullException("binder");

            using (_tempBinding.Create(binder, typeof (ScienceExperiment), config.Experiment))
            {
                var defaultSensorConfigNode =
                    config.SensorDefinition.With(sd => sd.GetNodeEx(typeof(DefaultExperimentSensor).Name));

                // Since we can handle any config, it's probably a good idea to mention if we're creating
                // this default sensor when the ConfigNode actually specifies a different one but its builder
                // wasn't found for some reason
                if (config.SensorDefinition.HasValue && !defaultSensorConfigNode.HasValue)
                {
                    if (config.SensorDefinition.Value.CountNodes > 1)
                        Log.Warning("Multiple unexpected subnodes inside " +
                                    config.SensorDefinition.Value.ToSafeString());

                    Log.Warning(config.SensorDefinition.Value.nodes
                        .Cast<ConfigNode>()
                        .FirstOrDefault()
                        .Return(n => n.name, "<unspecified>")
                        .Do(nodeName =>
                                Log.Warning(typeof (DefaultExperimentSensor).Name +
                                            " is being constructed because no builder that can handle " + nodeName +
                                            " was found")));
                }

                var onboardRule = GetRuleConfig(defaultSensorConfigNode, OnboardNodeName)
                    .Do(ruleNode => WarnUserIfUnhandledRuleNode(ruleNode, binder, config.Experiment.id, OnboardNodeName))
                    .With(c => CreateRule(c, binder))
                    .Or(() => CreateDefaultRule(_defaultOnboardConfig, binder));

                var availabilityRule = GetRuleConfig(defaultSensorConfigNode, AvailabilityNodeName)
                    .With(c => CreateRule(c, binder))
                    .Or(() => CreateDefaultRule(_defaultAvailabilityConfig, binder));

                var conditionRule = GetRuleConfig(defaultSensorConfigNode, ConditionNodeName)
                    .With(c => CreateRule(c, binder))
                    .Or(() => CreateDefaultRule(_defaultConditionConfig, binder));

                // Warn about unused nodes because I just wasted 5 minutes discovering I wrote AVAILABLE instead of AVAILABILITY in the
                // sensor definition
                defaultSensorConfigNode.Do(n => WarnAboutUnusedNodes(n, config.Experiment.id));

                    
                using (_tempBinding.Create(binder, typeof (ISensorRule), onboardRule, RuleKeys.Onboard))
                using (_tempBinding.Create(binder, typeof (ISensorRule), availabilityRule, RuleKeys.Availability))
                using (_tempBinding.Create(binder, typeof (ISensorRule), conditionRule, RuleKeys.Condition))
                using (_tempBinding.Create(binder, typeof(IScienceSubject), new DefaultScienceSubject()))
                using (_tempBinding.Create(binder, typeof (DefaultExperimentSensor)))
                    return binder.GetInstance<DefaultExperimentSensor>();
            }
        }


        private ISensorRule CreateDefaultRule([NotNull] ConfigNode config, IInjectionBinder binder)
        {
            try
            {
                return _ruleBuilder.Build(config, binder);
            }
            catch (Exception)
            {
                Log.Error("Failed to create default rule from " + config.ToSafeString());
                throw;
            }
        }


        private Maybe<ISensorRule> CreateRule([NotNull] ConfigNode ruleConfig, IInjectionBinder binder)
        {
            if (ruleConfig == null) throw new ArgumentNullException("ruleConfig");

            Log.Warning("Creating rule: " + ruleConfig.ToSafeString());

            return !_ruleBuilder.CanHandle(ruleConfig, binder) ? Maybe<ISensorRule>.None : _ruleBuilder.Build(ruleConfig, binder).ToMaybe();
        }


        public bool CanHandle(ExperimentConfiguration param1, IInjectionBinder param2, IObjectFromConfigNodeBuilder<IExperimentSensor, ExperimentConfiguration, IInjectionBinder> rootBuilder = null)
        {
            return true; // should always be able to create a default sensor, even if there is no info in the ConfigNode
        }


        private void WarnUserIfUnhandledRuleNode(
            Maybe<ConfigNode> ruleNode, 
            IInjectionBinder binder,
            string experimentId, 
            string whichRule)
        {
            if (ruleNode.HasValue && !_ruleBuilder.CanHandle(ruleNode.Value, binder))
                Log.Warning("Experiment " + experimentId + " " + whichRule +
                            " definition contains unsupported sensor rule type");
        }

        private static void WarnAboutUnusedNodes([NotNull] ConfigNode config, [NotNull] string experimentId)
        {
            if (config == null) throw new ArgumentNullException("config");
            if (experimentId == null) throw new ArgumentNullException("experimentId");

            // We use ONBOARD, AVAILABILITY, and CONDITION

            var unusedNodes = config.nodes
                                .Cast<ConfigNode>()
                                .Where(cn => new[] { OnboardNodeName, AvailabilityNodeName, ConditionNodeName }
                                                .All(ruleNodeName => !ruleNodeName.Equals(cn.name, StringComparison.Ordinal)));

                foreach (var n in unusedNodes)
                    Log.Warning("Unused node in sensor definition for " + experimentId + ": " + n.name);
        }


        private static Maybe<ConfigNode> GetRuleConfig(Maybe<ConfigNode> config, string nodeName)
        {
            if (!config.HasValue) return Maybe<ConfigNode>.None;

            var node = config.Value.GetNodeEx(nodeName);

            // NODENAME
            // {
            //
            // }

            if (!node.HasValue || node.Value.CountNodes == 0) return Maybe<ConfigNode>.None;

            if (node.Value.CountNodes == 1) return Maybe<ConfigNode>.With(node.Value.nodes[0]);

            var compositeConfig = new ConfigNode("ALL");
            foreach (ConfigNode subNode in node.Value.nodes)
                compositeConfig.AddNode(subNode);

            return Maybe<ConfigNode>.With(compositeConfig);
        }
    }

}
