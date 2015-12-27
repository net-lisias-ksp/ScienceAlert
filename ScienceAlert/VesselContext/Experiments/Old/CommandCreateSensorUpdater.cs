//using System;
//using strange.extensions.command.impl;
//using strange.extensions.context.api;
//using UnityEngine;

//namespace ScienceAlert.VesselContext.Experiments
//{
//// ReSharper disable once ClassNeverInstantiated.Global
//    public class CommandCreateSensorUpdater : Command
//    {
//        private readonly GameObject _vesselContext;
        
//        public CommandCreateSensorUpdater(
//            [Name(ContextKeys.CONTEXT_VIEW)] GameObject vesselContext)
//        {
//            if (vesselContext == null) throw new ArgumentNullException("vesselContext");
//            _vesselContext = vesselContext;
//        }


//        public override void Execute()
//        {
//            Log.Debug("Creating sensor updater");

//            var updater = _vesselContext.GetComponent<SensorManager>() ?? _vesselContext.AddComponent<SensorManager>();

//            injectionBinder.injector.Inject(updater, false);
//        }
//    }
//}
