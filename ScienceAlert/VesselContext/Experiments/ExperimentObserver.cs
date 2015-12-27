//using System;
//using ScienceAlert.VesselContext.Experiments.Sensors;

//namespace ScienceAlert.VesselContext.Experiments
//{
//    public class ExperimentObserver
//    {
//        private readonly ICollectionSensor _collectionSensor;
//        private readonly ITransmissionSensor _transmissionSensor;
//        private readonly ILabDataSensor _labSensor;

//        private float _collectionValue = 0f;
//        private float _transmissionValue = 0f;
//        private float _labValue = 0f;

//        public ExperimentObserver(
//            ICollectionSensor collectionSensor, 
//            ITransmissionSensor transmissionSensor,
//            ILabDataSensor labSensor)
//        {
//            if (collectionSensor == null) throw new ArgumentNullException("collectionSensor");
//            if (transmissionSensor == null) throw new ArgumentNullException("transmissionSensor");
//            if (labSensor == null) throw new ArgumentNullException("labSensor");

//            _collectionSensor = collectionSensor;
//            _transmissionSensor = transmissionSensor;
//            _labSensor = labSensor;
//        }


//        public void Update()
//        {
//            // just in case anything else queries sens
//            CheckCollectionSensor();
//            CheckTransmissionSensor();
//            CheckLabSensor();
//        }

//        private bool CheckCollectionSensor()
//        {
            
//        }

//        private bool CheckTransmissionSensor()
//        {
            
//        }

//        private bool CheckLabSensor()
//        {
            
//        }
//    }
//}
