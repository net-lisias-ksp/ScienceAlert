namespace ScienceAlert.VesselContext.Experiments
{
    public interface IExperimentSensorMonitorFactory
    {
        IExperimentSensorMonitor Create(ScienceExperiment experiment);
    }
}
