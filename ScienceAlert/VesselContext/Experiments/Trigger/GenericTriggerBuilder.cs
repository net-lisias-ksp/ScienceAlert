using ReeperCommon.Serialization;

namespace ScienceAlert.VesselContext.Experiments.Trigger
{
    class GenericTriggerBuilder<TTriggerType> : DefaultObjectFromConfigNodeBuilder<TTriggerType, ExperimentTrigger, ITriggerBuilder>,
        ITriggerBuilder where TTriggerType : ExperimentTrigger
    {
        public GenericTriggerBuilder(IConfigNodeSerializer serializer) : base(serializer)
        {
        }
    }
}
