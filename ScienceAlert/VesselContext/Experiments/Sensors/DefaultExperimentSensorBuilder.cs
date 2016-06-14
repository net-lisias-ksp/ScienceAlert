using System;
using System.Linq;
using JetBrains.Annotations;
using ReeperCommon.Containers;
using ReeperCommon.Logging;
using ReeperKSP.Extensions;
using strange.extensions.injector.api;
using ScienceAlert.Game;
using ScienceAlert.VesselContext.Experiments.Sensors.Rules;

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

                var onboardRule = GetRuleConfig(config.SensorDefinition, OnboardNodeName)
                    .With(c => CreateRule(c, binder))
                    .Or(() => CreateDefaultRule(_defaultOnboardConfig, binder));

                var availabilityRule = GetRuleConfig(config.SensorDefinition, AvailabilityNodeName)
                    .With(c => CreateRule(c, binder))
                    .Or(() => CreateDefaultRule(_defaultAvailabilityConfig, binder));

                var conditionRule = GetRuleConfig(config.SensorDefinition, ConditionNodeName)
                    .With(c => CreateRule(c, binder))
                    .Or(() => CreateDefaultRule(_defaultConditionConfig, binder));

                // Since we can handle any config, it's probably a good idea to mention if we're creating
                // this default sensor when the ConfigNode actually specifies a different one but its builder
                // wasn't found for some reason
                if (config.SensorDefinition.Any())
                {
                    if (config.SensorDefinition.Value.CountNodes > 1)
                        Log.Warning("Multiple unexpected subnodes inside " +
                                    config.SensorDefinition.Value.ToSafeString());

                    config.SensorDefinition.Value.nodes
                        .Cast<ConfigNode>()
                        .FirstOrDefault()
                        .With(n => n.name)
                        .If(
                            nodeName =>
                                !String.Equals(typeof (DefaultExperimentSensor).Name, nodeName,
                                    StringComparison.InvariantCultureIgnoreCase))
                        .Do(
                            nodeName =>
                                Log.Warning(typeof (DefaultExperimentSensor).Name +
                                            " is being constructed because no builder that can handle " + nodeName +
                                            " was found"));
                }

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
            
            return !_ruleBuilder.CanHandle(ruleConfig, binder) ? Maybe<ISensorRule>.None : _ruleBuilder.Build(ruleConfig, binder).ToMaybe();
        }


        public bool CanHandle(ExperimentConfiguration param1, IInjectionBinder param2, IObjectFromConfigNodeBuilder<IExperimentSensor, ExperimentConfiguration, IInjectionBinder> rootBuilder = null)
        {
            return true; // should always be able to create a default sensor, even if there is no info in the ConfigNode
        }


        private static Maybe<ConfigNode> GetRuleConfig(Maybe<ConfigNode> config, string nodeName)
        {
            if (!config.Any())
                return Maybe<ConfigNode>.None;
            
            var node = config.Value.GetNodeEx(nodeName);

            return node.Any() && node.Value.HasData ? node : Maybe<ConfigNode>.None;
        }
    }

}
