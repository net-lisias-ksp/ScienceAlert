using System;
using strange.extensions.command.impl;

namespace ScienceAlert.VesselContext.Gui
{
// ReSharper disable once ClassNeverInstantiated.Global
    public class CommandSaveGuiSettings : Command
    {
        private readonly SignalSaveGuiSettings _saveSignal;

        public CommandSaveGuiSettings(SignalSaveGuiSettings saveSignal)
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
