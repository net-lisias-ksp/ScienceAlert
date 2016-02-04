using System;
using ScienceAlert.Game;
using ScienceAlert.VesselContext.Experiments.Rules;

namespace ScienceAlert.VesselContext.Experiments
{
    public class ExperimentSensorFactory
    {
        private readonly IScienceSubjectProvider _subjectProvider;
        private readonly IExperimentReportValueCalculator _reportValueCalculator;
        private readonly ISensorRuleDefinitionSetProvider _ruleDefinitionProvider;

        private readonly IExperimentRuleFactory _ruleFactory;

        public ExperimentSensorFactory(
            IExperimentRuleFactory ruleFactory,
            IScienceSubjectProvider subjectProvider,
            IExperimentReportValueCalculator reportValueCalculator,
            ISensorRuleDefinitionSetProvider ruleDefinitionProvider
            )
        {
            if (ruleFactory == null) throw new ArgumentNullException("ruleFactory");
            if (subjectProvider == null) throw new ArgumentNullException("subjectProvider");
            if (reportValueCalculator == null) throw new ArgumentNullException("reportValueCalculator");
            if (ruleDefinitionProvider == null) throw new ArgumentNullException("ruleDefinitionProvider");

            _ruleFactory = ruleFactory;
            _subjectProvider = subjectProvider;
            _reportValueCalculator = reportValueCalculator;
            _ruleDefinitionProvider = ruleDefinitionProvider;
        }


        public ExperimentSensor Create(ScienceExperiment experiment)
        {
            if (experiment == null) throw new ArgumentNullException("experiment");

            try
            {
                var rulePackage =
                    _ruleDefinitionProvider.GetDefinitionSet(experiment)
                        .Or(_ruleDefinitionProvider.GetDefaultDefinition());

                return new ExperimentSensor(experiment, _subjectProvider, _reportValueCalculator,
                    _ruleFactory.Create(experiment, rulePackage.OnboardDefinition),
                    _ruleFactory.Create(experiment, rulePackage.AvailabilityDefinition),
                    _ruleFactory.Create(experiment, rulePackage.ConditionDefinition));
            }
            catch (Exception)
            {
                Log.Error("Exception thrown while creating sensor for " + experiment.id);
                throw;
            }
        }
    }
}
