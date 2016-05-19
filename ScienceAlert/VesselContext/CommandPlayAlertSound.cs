using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ReeperCommon.Logging;
using strange.extensions.command.impl;

namespace ScienceAlert.VesselContext
{
    class CommandPlayAlertSound : Command
    {
        private readonly SensorStatusChange _sensorStatus;

        public CommandPlayAlertSound(SensorStatusChange sensorStatus)
        {
            _sensorStatus = sensorStatus;
        }


        public override void Execute()
        {
            var newState = _sensorStatus.NewState;
            var oldState = _sensorStatus.OldState;

            var previousSubject = oldState.Subject.Id;
            var currentSubject = newState.Subject.Id;
            var canPerform = newState.Onboard && newState.Available && newState.ConditionsMet &&
                             newState.CollectionValue > 0f;
            var couldPerformLastTime = oldState.Onboard && oldState.Available && oldState.ConditionsMet &&
                                       oldState.CollectionValue > 0f;

            var shouldAlert = canPerform && (previousSubject != currentSubject || (!couldPerformLastTime));

            if (shouldAlert)
            {
                Log.Warning("Should play alert now");
            }
        }
    }
}
