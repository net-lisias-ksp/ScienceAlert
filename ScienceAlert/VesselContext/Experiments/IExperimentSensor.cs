namespace ScienceAlert.VesselContext.Experiments
{
    public interface IExperimentSensor
    {
        ScienceExperiment Experiment { get; }
        float CollectionValue { get; }
        float TransmissionValue { get; }
        float LabValue { get; }
    }
}