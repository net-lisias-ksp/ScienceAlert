using System;

namespace ScienceAlert.VesselContext.Experiments.Sensors.Queries
{
    public class ScienceTransmissionValueSensor : IQuerySensorValue<float>
    {
        private readonly ScienceExperiment _experiment;
        private readonly IQuerySensorValue<float> _collectionValueQuery;

        public ScienceTransmissionValueSensor(
            ScienceExperiment experiment,
            IQuerySensorValue<float> collectionValueQuery)
        {
            if (experiment == null) throw new ArgumentNullException("experiment");
            if (collectionValueQuery == null) throw new ArgumentNullException("collectionValueQuery");
            _experiment = experiment;
            _collectionValueQuery = collectionValueQuery;
        }


        public float Get()
        {
            throw new NotImplementedException();
        }
    }
}
