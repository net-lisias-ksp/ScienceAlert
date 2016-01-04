namespace ScienceAlert.VesselContext.Experiments.Sensors
{
    public interface IExperimentSensorFactory 
    {
        ISensor Create(ScienceExperiment experiment);
    }
}
