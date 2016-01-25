using ScienceAlert.Game;

namespace ScienceAlert.VesselContext.Experiments
{
    public interface IQueryScienceSubject
    {
        IScienceSubject GetSubject(ScienceExperiment experiment);
    }
}
