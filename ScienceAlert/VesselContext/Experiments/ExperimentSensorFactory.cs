using System;

namespace ScienceAlert.VesselContext.Experiments
{
    [Implements(typeof(IExperimentSensorFactory))]
    public class ExperimentSensorFactory : IExperimentSensorFactory
    {
        private readonly IQueryScienceSubject _subjectQuery;
        private readonly IExperimentReportValueCalculator _reportCalculator;

        public ExperimentSensorFactory(IQueryScienceSubject subjectQuery, IExperimentReportValueCalculator reportCalculator)
        {
            if (subjectQuery == null) throw new ArgumentNullException("subjectQuery");
            if (reportCalculator == null) throw new ArgumentNullException("reportCalculator");

            _subjectQuery = subjectQuery;
            _reportCalculator = reportCalculator;
        }


        public ExperimentSensor Create(ScienceExperiment experiment)
        {
            return new ExperimentSensor(experiment, _subjectQuery, _reportCalculator);
        }
    }
}
