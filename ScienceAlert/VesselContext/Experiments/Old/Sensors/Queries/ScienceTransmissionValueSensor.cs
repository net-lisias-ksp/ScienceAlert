//using System;
//using System.Linq;
//using ReeperCommon.Containers;
//using ScienceAlert.Core;
//using ScienceAlert.Game;

//namespace ScienceAlert.VesselContext.Experiments.Sensors.Queries
//{
//    public class ScienceTransmissionValueSensor : ScienceCollectionValueSensor
//    {
//        private readonly IScienceExperimentModuleCollectionProvider _scienceModuleProvider;
//        private readonly Lazy<float> _transmissionMultiplierQuery;
 
//        public ScienceTransmissionValueSensor(
//            ScienceExperiment experiment, 
//            IQueryScienceSubject subject, 
//            IQueryScienceValue queryScienceValue,
//            [Name(CoreKeys.CareerScienceGainMultiplier)] float careerScienceGainMultiplier, 
//            IScienceContainerCollectionProvider vesselContainer,
//            SignalActiveVesselModified vesselModifiedSignal,
//            IScienceExperimentModuleCollectionProvider scienceModuleProvider) : base(experiment, subject, queryScienceValue, careerScienceGainMultiplier, vesselContainer)
//        {
//            _scienceModuleProvider = scienceModuleProvider;
//            if (vesselModifiedSignal == null) throw new ArgumentNullException("vesselModifiedSignal");
//            if (scienceModuleProvider == null) throw new ArgumentNullException("scienceModuleProvider");

//            vesselModifiedSignal.AddListener(OnActiveVesselModified);
//            _transmissionMultiplierQuery = new Lazy<float>(GetBestTransmissionMultiplierForExperiment);
//        }


//        private void OnActiveVesselModified()
//        {
//            throw new NotImplementedException();
//        }


//        protected override float GetTransmissionMultiplier()
//        {
//            return _transmissionMultiplierQuery.Value;
//        }


//        private float GetBestTransmissionMultiplierForExperiment()
//        {
//            return _scienceModuleProvider.ScienceExperimentModules
//                .Where(mse => mse.ExperimentID == Experiment.id && mse.CanBeDeployed)
//                .Select(mse => mse.TransmissionMultiplier)
//                .OrderByDescending(mul => mul)
//                .first
                
//        }
//    }
//}
