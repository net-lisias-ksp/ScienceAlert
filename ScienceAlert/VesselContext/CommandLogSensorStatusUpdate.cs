using ReeperCommon.Logging;
using strange.extensions.command.impl;
using UnityEngine;

namespace ScienceAlert.VesselContext
{
// ReSharper disable once ClassNeverInstantiated.Global
    class CommandLogSensorStatusUpdate : Command
    {
        [Inject] public SensorStatusChange StatusChange { get; set; }

        public override void Execute()
        {
            Profiler.BeginSample("CommandLogSensorStatusUpdate.Execute");
            //Log.Verbose(GenerateSensorValueMessage);
            Profiler.EndSample();
        }


        private string GenerateSensorValueMessage()
        {
// ReSharper disable once ImpureMethodCallOnReadonlyValueField
            return string.Format("New sensor state: {0}", StatusChange.NewState);
        }
    }
}
