using ReeperCommon.Logging;
using strange.extensions.command.impl;

namespace ScienceAlert.VesselContext
{
// ReSharper disable once ClassNeverInstantiated.Global
    class CommandLogSensorStatusUpdate : Command
    {
        private readonly SensorStatusChange _statusChange;

        public CommandLogSensorStatusUpdate(SensorStatusChange statusChange)
        {
            _statusChange = statusChange;
        }


        public override void Execute()
        {
            Log.Verbose(GenerateSensorValueMessage);
        }


        private string GenerateSensorValueMessage()
        {
// ReSharper disable once ImpureMethodCallOnReadonlyValueField
            return string.Format("New sensor state: {0}", _statusChange.NewState);
        }
    }
}
