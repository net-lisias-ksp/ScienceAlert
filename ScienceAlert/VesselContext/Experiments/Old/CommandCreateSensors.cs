//using System;
//using System.Collections.Generic;
//using System.Linq;
//using strange.extensions.command.impl;

//namespace ScienceAlert.VesselContext.Experiments
//{
//// ReSharper disable once ClassNeverInstantiated.Global
//    public class CommandCreateSensors : Command
//    {
//        private readonly IExperimentSensorFactory _sensorFactory;
//        private readonly IEnumerable<ScienceExperiment> _experiments;
//        private readonly SignalShutdownScienceAlert _shutdownSignal;

//        public CommandCreateSensors(
//            IExperimentSensorFactory sensorFactory, 
//            IEnumerable<ScienceExperiment> experiments,
//            SignalShutdownScienceAlert shutdownSignal)
//        {
//            if (sensorFactory == null) throw new ArgumentNullException("sensorFactory");
//            if (experiments == null) throw new ArgumentNullException("experiments");
//            if (shutdownSignal == null) throw new ArgumentNullException("shutdownSignal");
//            _sensorFactory = sensorFactory;
//            _experiments = experiments;
//            _shutdownSignal = shutdownSignal;
//        }


//        public override void Execute()
//        {
//            try
//            {
//                var sensors = _experiments
//                    .Select(e => _sensorFactory.Create(e))
//                    .ToDictionary(kvp => kvp.Key, kvp => kvp.Value); 

//                injectionBinder.Bind<IEnumerable<ISensor>>().ToValue(sensors.Keys.ToList());
//                injectionBinder.Bind<IEnumerable<ISensorValues>>().ToValue(sensors.Values.ToList());
//                injectionBinder.Bind<Dictionary<ISensor, ISensorValues>>().ToValue(sensors);
//            }
//            catch (Exception e)
//            {
//                Log.Error("Error while creating sensors: " + e);
//                Fail();
//                _shutdownSignal.Dispatch();
//            }    
//        }
//    }
//}
