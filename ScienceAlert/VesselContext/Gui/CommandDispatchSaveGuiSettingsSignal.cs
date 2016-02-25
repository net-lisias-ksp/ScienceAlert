using System;
using ReeperCommon.Logging;
using strange.extensions.command.impl;

namespace ScienceAlert.VesselContext.Gui
{
// ReSharper disable once ClassNeverInstantiated.Global
    public class CommandDispatchSaveGuiSettingsSignal : Command
    {
        private readonly SignalSaveGuiSettings _saveSignal;

        public CommandDispatchSaveGuiSettingsSignal(SignalSaveGuiSettings saveSignal)
        {
            if (saveSignal == null) throw new ArgumentNullException("saveSignal");
            _saveSignal = saveSignal;
        }

        public override void Execute()
        {
            Log.Verbose("Dispatching gui save signal");
            _saveSignal.Dispatch();
        }
    }
}
