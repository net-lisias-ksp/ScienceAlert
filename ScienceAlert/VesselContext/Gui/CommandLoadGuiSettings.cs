using System;
using strange.extensions.command.impl;

namespace ScienceAlert.VesselContext.Gui
{
// ReSharper disable once ClassNeverInstantiated.Global
    public class CommandLoadGuiSettings : Command
    {
        private readonly SignalLoadGuiSettings _loadSignal;

        public CommandLoadGuiSettings(SignalLoadGuiSettings loadSignal)
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
