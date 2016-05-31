using strange.extensions.command.impl;
using ScienceAlert.VesselContext.Experiments;

namespace ScienceAlert.VesselContext
{
    class CommandUpdateAlertStatus : Command
    {
        [Inject] public SensorStatusChange SensorStatus { get; set; }
        [Inject] public SignalExperimentAlertChanged AlertStatusChanged { get; set; }
        [Inject] public IAlertStateCache AlertStateCache { get; set; }
        [Inject] public ExperimentIdentifierProvider IdentifierProvider { get; set; }

        public override void Execute()
        {
            // todo: support transmission and lab alerts
            // right now, only collection implemented

            var newState = SensorStatus.CurrentState;
            var oldState = SensorStatus.PreviousState;
            var oldAlertStatus = AlertStateCache.GetStatus(IdentifierProvider.Get(newState.Experiment));

            var previousSubject = oldState.Subject.Id;
            var currentSubject = newState.Subject.Id;
            var canPerform = newState.Onboard && newState.Available && newState.ConditionsMet &&
                             newState.CollectionValue > 0f;
            var couldPerformLastTime = oldState.Onboard && oldState.Available && oldState.ConditionsMet &&
                                       oldState.CollectionValue > 0f;
            var shouldAlert = canPerform && (previousSubject != currentSubject || !couldPerformLastTime);

            var collectionAlert = shouldAlert && newState.CollectionValue > 0f;
            var transmissionAlert = shouldAlert && newState.TransmissionValue > 0f;
            var labAlert = shouldAlert && newState.LabValue > 0f;

            var newAlertStatus = ExperimentAlertStatus.None;

            newAlertStatus |= collectionAlert ? ExperimentAlertStatus.Collection : 0;
            newAlertStatus |= transmissionAlert ? ExperimentAlertStatus.Transmission : 0;
            newAlertStatus |= labAlert ? ExperimentAlertStatus.Lab : 0;

            if (newAlertStatus == ExperimentAlertStatus.None)
            {
                if (newAlertStatus != oldAlertStatus)
                    AlertStatusChanged.Dispatch(SensorStatus, new AlertStatusChange(oldAlertStatus, newAlertStatus));
                Fail();
            }
            else
            {
                AlertStatusChanged.Dispatch(SensorStatus, new AlertStatusChange(oldAlertStatus, newAlertStatus));
            }
        }
    }
}
