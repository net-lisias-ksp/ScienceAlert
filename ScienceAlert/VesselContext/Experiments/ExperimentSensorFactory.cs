using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ReeperCommon.Containers;
using ScienceAlert.Core;
using ScienceAlert.Game;
using ScienceAlert.VesselContext.Experiments.Rules;

namespace ScienceAlert.VesselContext.Experiments
{
    public class ExperimentSensorFactory
    {
        private readonly IScienceSubjectProvider _subjectProvider;
        private readonly IExperimentReportValueCalculator _reportValueCalculator;
        private readonly IExperimentRuleFactory _ruleFactory;
        private readonly IRuleDefinitionSetProvider _ruleDefinitionSetProvider;


        public ExperimentSensorFactory(
            IScienceSubjectProvider subjectProvider,
            IExperimentReportValueCalculator reportValueCalculator,
            IExperimentRuleFactory ruleFactory,
            IRuleDefinitionSetProvider ruleDefinitionSetProvider
            )
        {
            if (subjectProvider == null) throw new ArgumentNullException("subjectProvider");
            if (reportValueCalculator == null) throw new ArgumentNullException("reportValueCalculator");
            if (ruleFactory == null) throw new ArgumentNullException("ruleFactory");
            if (ruleDefinitionSetProvider == null) throw new ArgumentNullException("ruleDefinitionSetProvider");

            _subjectProvider = subjectProvider;
            _reportValueCalculator = reportValueCalculator;
            _ruleFactory = ruleFactory;
            _ruleDefinitionSetProvider = ruleDefinitionSetProvider;
        }


        public ExperimentSensor Create(ScienceExperiment experiment)
        {
            if (experiment == null) throw new ArgumentNullException("experiment");

            var ruleset = _ruleDefinitionSetProvider.GetRuleDefinitionSetFor(experiment);

            if (!ruleset.Any())
                throw new ExperimentRuleDefinitionSetNotFoundException(experiment);

            return new ExperimentSensor(experiment, _subjectProvider, _reportValueCalculator,
                _ruleFactory.Create(experiment, ruleset.Value.OnboardRuleDefinition),
                _ruleFactory.Create(experiment, ruleset.Value.AvailabilityRuleDefinition),
                _ruleFactory.Create(experiment, ruleset.Value.ConditionRuleDefinition));
        }
    }
}
