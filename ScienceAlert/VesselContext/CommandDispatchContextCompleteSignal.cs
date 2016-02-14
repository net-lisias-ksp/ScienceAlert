//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using strange.extensions.command.impl;

//namespace ScienceAlert.VesselContext
//{
//    class CommandDispatchContextCompleteSignal : Command
//    {
//        private readonly SignalContextComplete _signal;

//        public CommandDispatchContextCompleteSignal(SignalContextComplete signal)
//        {
//            if (signal == null) throw new ArgumentNullException("signal");
//            _signal = signal;
//        }

//        public override void Execute()
//        {
//            _signal.Dispatch();
//        }
//    }
//}
