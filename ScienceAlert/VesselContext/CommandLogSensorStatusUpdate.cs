using ReeperCommon.Logging;
using strange.extensions.command.impl;

namespace ScienceAlert.VesselContext
{
// ReSharper disable once ClassNeverInstantiated.Global
    class CommandLogSensorStatusUpdate : Command
    {
        private readonly ExperimentSensorState _sensorState;

        public CommandLogSensorStatusUpdate(ExperimentSensorState sensorState)
        {
            _sensorState = sensorState;
        }


        public override void Execute()
        {
            Log.Verbose(GenerateSensorValueMessage);
        }


        private string GenerateSensorValueMessage()
        {
// ReSharper disable once ImpureMethodCallOnReadonlyValueField
            return string.Format("New values: {0}", _sensorState.ToString());
        }
    }
}
