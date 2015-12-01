namespace ScienceAlert.Experiments
{
    public interface IExperimentSensorFactory
    {
        IExperimentSensor Create(ScienceExperiment experiment);
    }
}
