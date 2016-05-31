using System;
using JetBrains.Annotations;
using strange.extensions.command.impl;

namespace ScienceAlert.VesselContext
{
    class CommandPlayAlertSound : Command
    {
        private readonly PlayableSound _alertClip;
        private readonly AlertStatusChange _alertStatusChange;

        public CommandPlayAlertSound([NotNull, Name(CrossContextKeys.AlertSound)] PlayableSound alertClip, AlertStatusChange alertStatusChange)
        {
            if (alertClip == null) throw new ArgumentNullException("alertClip");
            _alertClip = alertClip;
            _alertStatusChange = alertStatusChange;
        }

        public override void Execute()
        {
            if (_alertStatusChange.CurrentStatus == ExperimentAlertStatus.None)
                return;

            _alertClip.Play();
        }
    }
}
