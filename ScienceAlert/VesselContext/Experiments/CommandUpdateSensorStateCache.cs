using strange.extensions.command.impl;

namespace ScienceAlert.VesselContext.Experiments
{
    // ReSharper disable once ClassNeverInstantiated.Global
    class CommandUpdateSensorStateCache : Command
    {
        [Inject] public ExperimentStateCache Cache { get; set; }
        [Inject] public SensorStatusChange StatusChange { get; set; }

        public override void Execute()
        {
            Cache.UpdateState(StatusChange.NewState);
        }
    }
}
