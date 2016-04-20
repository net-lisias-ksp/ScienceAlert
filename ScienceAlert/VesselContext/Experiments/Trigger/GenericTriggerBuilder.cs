using ReeperCommon.Serialization;
using strange.extensions.injector.api;

namespace ScienceAlert.VesselContext.Experiments.Trigger
{
    class GenericTriggerBuilder<TTriggerType> : DefaultObjectFromConfigNodeBuilder<TTriggerType, ExperimentTrigger, ITriggerBuilder, IInjectionBinder>,
        ITriggerBuilder where TTriggerType : ExperimentTrigger
    {
        public GenericTriggerBuilder(IConfigNodeSerializer serializer) : base(serializer)
        {
        }
    }
}
