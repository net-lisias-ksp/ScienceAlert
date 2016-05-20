using ReeperCommon.Logging;
using strange.extensions.command.impl;

namespace ScienceAlert.VesselContext
{
// ReSharper disable once ClassNeverInstantiated.Global
    class CommandLogSensorStatusUpdate : Command
    {
        [Inject] public SensorStatusChange StatusChange { get; set; }

        public override void Execute()
        {
            Log.Verbose(GenerateSensorValueMessage);
        }


        private string GenerateSensorValueMessage()
        {
// ReSharper disable once ImpureMethodCallOnReadonlyValueField
            return string.Format("New sensor state: {0}", StatusChange.NewState);
        }
    }
}
