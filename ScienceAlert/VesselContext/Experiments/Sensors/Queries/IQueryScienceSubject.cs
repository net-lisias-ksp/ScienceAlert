namespace ScienceAlert.VesselContext.Experiments.Sensors.Queries
{
    public interface IQueryScienceSubject
    {
        ScienceSubject GetSubject(ScienceExperiment experiment);
    }
}
