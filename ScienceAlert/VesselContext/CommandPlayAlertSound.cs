using System;
using JetBrains.Annotations;
using ReeperCommon.Logging;
using strange.extensions.command.impl;
using UnityEngine;

namespace ScienceAlert.VesselContext
{
    class CommandPlayAlertSound : Command
    {
        private readonly PlayableSound _alertClip;

        public CommandPlayAlertSound([NotNull, Name(CrossContextKeys.AlertSound)] PlayableSound alertClip)
        {
            if (alertClip == null) throw new ArgumentNullException("alertClip");
            _alertClip = alertClip;
        }

        public override void Execute()
        {
            Profiler.BeginSample("CommandPlayAlertSound.Execute");

            //Log.Verbose("Playing alert");
            _alertClip.Play();

            Profiler.EndSample();
        }
    }
}
