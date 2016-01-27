using System;
using ScienceAlert.Game;

namespace ScienceAlert.VesselContext.Experiments
{
    [Implements(typeof(IExperimentSensorFactory))]
    public class ExperimentSensorFactory : IExperimentSensorFactory
    {
        private readonly IScienceSubjectProvider _subjectProvider;
        private readonly IExperimentReportValueCalculator _reportCalculator;

        public ExperimentSensorFactory(IScienceSubjectProvider subjectProvider, IExperimentReportValueCalculator reportCalculator)
        {
            if (subjectProvider == null) throw new ArgumentNullException("subjectProvider");
            if (reportCalculator == null) throw new ArgumentNullException("reportCalculator");

            _subjectProvider = subjectProvider;
            _reportCalculator = reportCalculator;
        }


        public ExperimentSensor Create(ScienceExperiment experiment)
        {
            return new ExperimentSensor(experiment, _subjectProvider, _reportCalculator);
        }
    }
}
