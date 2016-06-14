using JetBrains.Annotations;
using strange.extensions.injector.api;
using ScienceAlert.VesselContext.Experiments.Sensors.Rules;

namespace ScienceAlert.VesselContext.Experiments.Sensors
{
    [RegisterBuilder(typeof(IObjectFromConfigNodeBuilder<IExperimentSensor, ExperimentConfiguration, IInjectionBinder>))]
    // ReSharper disable once UnusedMember.Global
    class EvaExperimentSensorBuilder : DefaultExperimentSensorBuilder
    {
        public EvaExperimentSensorBuilder(
          [NotNull] IObjectFromConfigNodeBuilder<ISensorRule, ConfigNode, IInjectionBinder> ruleBuilder,
            [NotNull, Name(CrossContextKeys.DefaultOnboardRule)] ConfigNode defaultOnboardConfig,
            [NotNull, Name(CrossContextKeys.DefaultAvailabilityRule)] ConfigNode defaultAvailabilityConfig,
            [NotNull, Name(CrossContextKeys.DefaultConditionRule)] ConfigNode defaultConditionConfig,
            [NotNull] ITemporaryBindingFactory tempBinding)
            : base(
                ruleBuilder, defaultOnboardConfig, defaultAvailabilityConfig, defaultConditionConfig, tempBinding)
        {
            
        }
    }
}
