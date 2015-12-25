using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ScienceAlert.VesselContext.Experiments;
using strange.extensions.command.impl;

namespace ScienceAlert.VesselContext
{
    public class CommandLogSensorStatusUpdate : Command
    {
        private readonly ScienceExperiment _experiment;
        private readonly ISensorState _sensorState;

        public CommandLogSensorStatusUpdate(ScienceExperiment experiment, ISensorState sensorState)
        {
            if (experiment == null) throw new ArgumentNullException("experiment");
            if (sensorState == null) throw new ArgumentNullException("sensorState");
            _experiment = experiment;
            _sensorState = sensorState;
        }


        public override void Execute()
        {
            Log.Verbose(GenerateSensorValueMessage);
        }


        private string GenerateSensorValueMessage()
        {
            return _experiment.id + " status values: " + _sensorState.ToString();
        }
    }
}
