namespace ScienceAlert.VesselContext.Experiments.Sensors
{
    public interface ISensorFactory
    {
        IOnboardSensor CreateOnboardSensor(ScienceExperiment experiment);
        IAvailabilitySensor CreateAvailabilitySensor(ScienceExperiment experiment);
        ICollectionSensor CreateCollectionSensor(ScienceExperiment experiment);
        ITransmissionSensor CreateTransmissionSensor(ScienceExperiment experiment);
        ILabDataSensor CreateLabDataSensor(ScienceExperiment experiment);
    }
}
