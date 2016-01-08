using System;
using System.Collections.ObjectModel;
using System.Linq;
using ScienceAlert.Game;

namespace ScienceAlert.VesselContext.Experiments.Sensors.Queries
{
    [Implements(typeof(IQueryScienceDataForScienceSubject))]
// ReSharper disable once UnusedMember.Global
    public class QueryScienceDataForScienceSubject : IQueryScienceDataForScienceSubject
    {
        private readonly IScienceContainerCollectionProvider _scienceContainerCollection;

        public QueryScienceDataForScienceSubject(IScienceContainerCollectionProvider scienceContainerCollection)
        {
            if (scienceContainerCollection == null) throw new ArgumentNullException("scienceContainerCollection");
            _scienceContainerCollection = scienceContainerCollection;
        }


        public ReadOnlyCollection<ScienceData> GetScienceData(ScienceSubject targetSubject)
        {
            if (targetSubject == null) throw new ArgumentNullException("targetSubject");

            var tSubject = targetSubject;

            return
                new ReadOnlyCollection<ScienceData>(_scienceContainerCollection.Containers
                    .SelectMany(
                        container => container.GetScienceCount() > 0 ? container.GetData() : Enumerable.Empty<ScienceData>())
                    .Where(sd => sd.subjectID == tSubject.id).ToList());
        }
    }
}
