//using System;
//using System.Collections.Generic;
//using strange.extensions.command.impl;

//namespace ScienceAlert.VesselContext.Gui
//{
//    class CommandTriggerInitialSensorUpdates : Command
//    {
//        private readonly IEnumerable<ScienceExperiment> _experiments;
//        private readonly SignalTriggerSensorStatusUpdate _triggerSignal;

//        public CommandTriggerInitialSensorUpdates(IEnumerable<ScienceExperiment> experiments,
//            SignalTriggerSensorStatusUpdate triggerSignal)
//        {
//            if (experiments == null) throw new ArgumentNullException("experiments");
//            if (triggerSignal == null) throw new ArgumentNullException("triggerSignal");
//            _experiments = experiments;
//            _triggerSignal = triggerSignal;
//        }


//        public override void Execute()
//        {
//            foreach (var exp in _experiments)
//                _triggerSignal.Dispatch(exp);
//        }
//    }
//}
