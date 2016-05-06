using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using ReeperCommon.Containers;
using ReeperCommon.Logging;
using ReeperKSP.Extensions;
using strange.extensions.command.impl;
using ScienceAlert.Game;
using ScienceAlert.SensorDefinitions;
using ScienceAlert.VesselContext.Experiments.Rules;

namespace ScienceAlert.VesselContext.Experiments
{
// ReSharper disable once ClassNeverInstantiated.Global
    class CommandCreateExperimentSensors : Command
    {
        private readonly IRuleFactory _ruleFactory;
        private readonly ReadOnlyCollection<SensorDefinition> _sensorDefinitions;
        private readonly IScienceSubjectProvider _subjectProvider;
        private readonly IExperimentReportValueCalculator _reportCalculator;
        private readonly ITemporaryBindingFactory _temporaryBindingFactory;
        private readonly SignalCriticalShutdown _failSignal;

        public CommandCreateExperimentSensors(
            IRuleFactory ruleFactory,
            ReadOnlyCollection<SensorDefinition> sensorDefinitions,
            IScienceSubjectProvider subjectProvider,
            IExperimentReportValueCalculator reportCalculator,
            ITemporaryBindingFactory temporaryBindingFactory,
            SignalCriticalShutdown failSignal)
        {
            if (ruleFactory == null) throw new ArgumentNullException("ruleFactory");
            if (sensorDefinitions == null) throw new ArgumentNullException("sensorDefinitions");
            if (subjectProvider == null) throw new ArgumentNullException("subjectProvider");
            if (reportCalculator == null) throw new ArgumentNullException("reportCalculator");
            if (temporaryBindingFactory == null) throw new ArgumentNullException("temporaryBindingFactory");
            if (failSignal == null) throw new ArgumentNullException("failSignal");

            _ruleFactory = ruleFactory;
            _sensorDefinitions = sensorDefinitions;
            _subjectProvider = subjectProvider;
            _reportCalculator = reportCalculator;
            _temporaryBindingFactory = temporaryBindingFactory;
            _failSignal = failSignal;
        }


        public override void Execute()
        {
            var sensors = new List<ExperimentSensor>();

            try
            {
                sensors = CreateSensors().ToList();

                if (!sensors.Any())
                {
                    Log.Error(
                        "Failed to create any experiment sensors -- something is wrong. ScienceAlert cannot work as expected");
                    _failSignal.Dispatch();
                    Fail();
                    return;
                }
            }
            finally
            {
                injectionBinder
                    .Bind<IEnumerable<ExperimentSensor>>()
                    .Bind<List<ExperimentSensor>>()
                        .To(sensors);
                injectionBinder.Bind<ReadOnlyCollection<ExperimentSensor>>().To(sensors.AsReadOnly());

                Log.Verbose("Created " + sensors.Count + " experiment sensors");
            }    
        }


        private IEnumerable<ExperimentSensor> CreateSensors()
        {
            var sensors = new List<ExperimentSensor>();


            foreach (var sensorDefinition in _sensorDefinitions)
            {
                try
                {
                    injectionBinder.Bind<ScienceExperiment>().To(sensorDefinition.Experiment);

                    sensors.Add(CreateSensor(sensorDefinition));
                }
                catch (Exception e)
                {
                    Log.Error("Failed to create experiment sensor for " + sensorDefinition);
                    Log.Error("Encountered exception: " + e);
                }
                finally
                {
                    injectionBinder.Unbind<ScienceExperiment>();
                }
            }

            Log.Debug("Created " + sensors.Count + " of " + _sensorDefinitions.Count() + " successfully");
            return sensors;
        }


        private ExperimentSensor CreateSensor(SensorDefinition definition)
        {
            if (definition == null) throw new ArgumentNullException("definition");

            return new ExperimentSensor(definition.Experiment,
                _subjectProvider,
                _reportCalculator,
                CreateRule(definition.OnboardRuleDefinition),
                CreateRule(definition.AvailabilityRuleDefinition),
                CreateRule(definition.ConditionRuleDefinition));
        }


        private IExperimentRule CreateRule(ConfigNode ruleConfig)
        {
            if (ruleConfig == null) throw new ArgumentNullException("ruleConfig");

            if (!_ruleFactory.CanHandle(ruleConfig))
                throw new ArgumentException("No builder for " + ruleConfig.ToSafeString());


            return _ruleFactory.With(b => b.Build(ruleConfig, _ruleFactory, injectionBinder, _temporaryBindingFactory));
        }
    }
}
