using System;
using strange.extensions.command.impl;

namespace ScienceAlert.VesselContext.Gui
{
// ReSharper disable once ClassNeverInstantiated.Global
    public class CommandDispatchLoadGuiSettingsSignal : Command
    {
        private readonly SignalLoadGuiSettings _loadSignal;

        public CommandDispatchLoadGuiSettingsSignal(SignalLoadGuiSettings loadSignal)
        {
            if (loadSignal == null) throw new ArgumentNullException("loadSignal");
            _loadSignal = loadSignal;
        }


        public override void Execute()
        {
            Log.Verbose("Dispatching gui load signal");
            _loadSignal.Dispatch();
        }
    }
}
