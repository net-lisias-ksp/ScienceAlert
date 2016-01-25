namespace ScienceAlert.VesselContext.Experiments
{
    public interface IExperimentSensorFactory
    {
        ExperimentSensor Create(ScienceExperiment experiment);
    }
}
