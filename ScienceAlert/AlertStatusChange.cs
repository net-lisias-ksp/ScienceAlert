using System;

namespace ScienceAlert
{
    [Flags]
    public enum ExperimentAlertStatus
    {
        None = 0 << 0,
        Collection = 1 << 0,
        Transmission = 1 << 1,
        Lab = 1 << 2
    }

    public struct AlertStatusChange
    {
        public readonly ExperimentAlertStatus PreviousStatus;
        public readonly ExperimentAlertStatus CurrentStatus;

        public AlertStatusChange(ExperimentAlertStatus oldStatus, ExperimentAlertStatus newStatus) : this()
        {
            PreviousStatus = oldStatus;
            CurrentStatus = newStatus;
        }
    }
}
