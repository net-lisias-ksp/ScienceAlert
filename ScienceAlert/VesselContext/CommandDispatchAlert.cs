using ReeperCommon.Logging;
using strange.extensions.command.impl;
using UnityEngine;

namespace ScienceAlert.VesselContext
{
    class CommandDispatchAlert : Command
    {
        [Inject] public SensorStatusChange SensorStatus { get; set; }
        [Inject] public SignalScienceAlertIssued ScienceAlertSignal { get; set; }
        
        public override void Execute()
        {
            Profiler.BeginSample("CommandDispatchAlert.Execute");

            var newState = SensorStatus.NewState;
            var oldState = SensorStatus.OldState;

            var previousSubject = oldState.Subject.Id;
            var currentSubject = newState.Subject.Id;
            var canPerform = newState.Onboard && newState.Available && newState.ConditionsMet &&
                             newState.CollectionValue > 0f;
            var couldPerformLastTime = oldState.Onboard && oldState.Available && oldState.ConditionsMet &&
                                       oldState.CollectionValue > 0f;

            var shouldAlert = canPerform && (previousSubject != currentSubject || (!couldPerformLastTime));

            if (!shouldAlert)
            {
                //Log.Debug("Not alerting; new state " + newState + "; old state " + oldState);
                Fail();
            }
            else
            {
                //Log.Verbose("Issuing alert for " + SensorStatus.NewState.Experiment.id);
                ScienceAlertSignal.Dispatch();
            }

            Profiler.EndSample();
        }
    }
}
