using strange.extensions.command.impl;
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace ScienceAlert.VesselContext.Experiments
{
    // ReSharper disable once ClassNeverInstantiated.Global
    class CommandUpdateAlertStatus : Command
    {
        [Inject] public SensorStatusChange SensorStatus { get; set; }
        [Inject] public SignalExperimentAlertChanged AlertStatusChanged { get; set; }
        [Inject] public IAlertStateCache AlertStateCache { get; set; }
        [Inject] public ExperimentIdentifierProvider IdentifierProvider { get; set; }

        public override void Execute()
        {
            var newState = SensorStatus.CurrentState;
            var oldState = SensorStatus.PreviousState;
            var oldAlertStatus = AlertStateCache.GetStatus(IdentifierProvider.Get(newState.Experiment));

            var previousSubject = oldState.Subject.Id;
            var currentSubject = newState.Subject.Id;
            var canPerform = newState.Onboard && newState.Available && newState.ConditionsMet &&
                             newState.RecoveryValue > 0f;
            var couldPerformLastTime = oldState.Onboard && oldState.Available && oldState.ConditionsMet &&
                                       oldState.RecoveryValue > 0f;
            var shouldAlert = canPerform && (previousSubject != currentSubject || !couldPerformLastTime);

            var recoveryAlert = shouldAlert && newState.RecoveryValue > 0f;
            var transmissionAlert = shouldAlert && newState.TransmissionValue > 0f;
            var labAlert = shouldAlert && newState.LabValue > 0f;

            var newAlertStatus = ExperimentAlertStatus.None;

            newAlertStatus |= recoveryAlert ? ExperimentAlertStatus.Recovery : 0;
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
