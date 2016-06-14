using System;
using System.Linq;
using JetBrains.Annotations;
using ReeperCommon.Containers;
using ReeperKSP.Extensions;
using strange.extensions.injector.api;
using ScienceAlert.VesselContext.Experiments;

namespace ScienceAlert.Core
{
    [RegisterBuilder(typeof(IObjectFromConfigNodeBuilder<ExperimentConfiguration, ConfigNode, IInjectionBinder>), int.MaxValue)]
    public class DefaultExperimentConfigurationBuilder :
        IObjectFromConfigNodeBuilder<ExperimentConfiguration, ConfigNode, IInjectionBinder>
    {
        private readonly string _configurationNodeName;

        private const string ExperimentIdValueName = "experimentID";
        private const string SensorNodeName = "Sensor";
        private const string TriggerValueName = "trigger";

        public DefaultExperimentConfigurationBuilder(
            [NotNull, Name(CoreContextKeys.ExperimentConfigurationNodeName)] string configurationNodeName)
        {
            if (configurationNodeName == null) throw new ArgumentNullException("configurationNodeName");
            _configurationNodeName = configurationNodeName;
        }

        public ExperimentConfiguration Build([NotNull] ConfigNode param1, [NotNull] IInjectionBinder param2, IObjectFromConfigNodeBuilder<ExperimentConfiguration, ConfigNode, IInjectionBinder> rootBuilder = null)
        {
            if (!CanHandle(param1, param2, rootBuilder))
                throw new ArgumentException("This builder cannot handle the given parameter");

            if (param1 == null) throw new ArgumentNullException("param1");
            if (param2 == null) throw new ArgumentNullException("param2");

            var experiment = GetExperimentId(param1)
                .With(GetExperimentFromId);

            if (!experiment.Any()) throw new ArgumentException("ConfigNode does not specify an experimentID");

            return new ExperimentConfiguration(experiment.Value, LoadSensorConfig(param1), GetTrigger(param1));
        }


        public bool CanHandle(ConfigNode param1, IInjectionBinder param2, IObjectFromConfigNodeBuilder<ExperimentConfiguration, ConfigNode, IInjectionBinder> rootBuilder = null)
        {
            return param1 != null && param1.name == _configurationNodeName && GetExperimentId(param1).Any();
        }


        private static Maybe<string> GetTrigger(ConfigNode config)
        {
            return config.GetValueEx(TriggerValueName);
        }


        private static Maybe<ConfigNode> LoadSensorConfig(ConfigNode config)
        {
            return config.GetNodeEx(SensorNodeName);
        }


        private static Maybe<string> GetExperimentId(ConfigNode config)
        {
            return config.GetValueEx(ExperimentIdValueName);
        }


        private static Maybe<ScienceExperiment> GetExperimentFromId(string experimentId)
        {
            if (string.IsNullOrEmpty(experimentId))
                return Maybe<ScienceExperiment>.None;
            return ResearchAndDevelopment.GetExperimentIDs().All(id => id != experimentId) ? Maybe<ScienceExperiment>.None : ResearchAndDevelopment.GetExperiment(experimentId).ToMaybe();
        } 
    }
}