using System.Collections.ObjectModel;

namespace ScienceAlert.VesselContext.Experiments.Sensors.Queries
{
    public interface IQueryScienceDataForScienceSubject
    {
        ReadOnlyCollection<ScienceData> GetScienceData(ScienceSubject targetSubject);
    }
}
