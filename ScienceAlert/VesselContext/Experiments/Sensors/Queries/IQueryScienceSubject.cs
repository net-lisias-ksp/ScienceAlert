using ScienceAlert.Game;

namespace ScienceAlert.VesselContext.Experiments.Sensors.Queries
{
    public interface IQueryScienceSubject
    {
        IScienceSubject GetSubject(ScienceExperiment experiment);
    }
}
