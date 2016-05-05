//namespace ScienceAlert.VesselContext.Gui
//{
//// ReSharper disable once ClassNeverInstantiated.Global
//    public class ExperimentListEntryFactory
//    {
//        public ExperimentListEntry Create(ExperimentSensorState sensorState)
//        {
//            var canBeDeployed = sensorState.Onboard && sensorState.Available && sensorState.ConditionsMet;

//            return new ExperimentListEntry(
//                sensorState.Experiment,
//                sensorState.Experiment.experimentTitle,
//                canBeDeployed,                              // deploy button enabled?
//                true,                                       // display in experiment list?
//                sensorState.CollectionValue,
//                false,                                      // collection alert?
//                sensorState.TransmissionValue,
//                false,                                      // transmission alert?
//                sensorState.LabValue,
//                false);                                     // lab alert?
//        }
//    }
//}
