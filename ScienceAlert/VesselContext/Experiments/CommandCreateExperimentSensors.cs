using System;
using System.Collections.Generic;
using System.Linq;
using ReeperCommon.Containers;
using ReeperCommon.Extensions;
using ReeperCommon.Logging;
using ReeperCommon.Serialization;
using ScienceAlert.Game;
using ScienceAlert.SensorDefinitions;
using ScienceAlert.VesselContext.Experiments.Rules;
using strange.extensions.command.impl;

namespace ScienceAlert.VesselContext.Experiments
{
// ReSharper disable once ClassNeverInstantiated.Global
    public class CommandCreateExperimentSensors : Command
    {
        private readonly IRuleBuilderProvider _ruleBuilderProvider;
        private readonly IConfigNodeSerializer _serializer;
        private readonly IEnumerable<SensorDefinition> _sensorDefinitions;
        private readonly IScienceSubjectProvider _subjectProvider;
        private readonly IExperimentReportValueCalculator _reportCalculator;
        private readonly ITemporaryBindingFactory _temporaryBindingFactory;
        private readonly SignalCriticalShutdown _failSignal;

        public CommandCreateExperimentSensors(
            IRuleBuilderProvider ruleBuilderProvider,
            IConfigNodeSerializer serializer,
            IEnumerable<SensorDefinition> sensorDefinitions,
            IScienceSubjectProvider subjectProvider,
            IExperimentReportValueCalculator reportCalculator,
            ITemporaryBindingFactory temporaryBindingFactory,
            SignalCriticalShutdown failSignal)
        {
            if (ruleBuilderProvider == null) throw new ArgumentNullException("ruleBuilderProvider");
            if (serializer == null) throw new ArgumentNullException("serializer");
            if (sensorDefinitions == null) throw new ArgumentNullException("sensorDefinitions");
            if (subjectProvider == null) throw new ArgumentNullException("subjectProvider");
            if (reportCalculator == null) throw new ArgumentNullException("reportCalculator");
            if (temporaryBindingFactory == null) throw new ArgumentNullException("temporaryBindingFactory");
            if (failSignal == null) throw new ArgumentNullException("failSignal");

            _ruleBuilderProvider = ruleBuilderProvider;
            _serializer = serializer;
            _sensorDefinitions = sensorDefinitions;
            _subjectProvider = subjectProvider;
            _reportCalculator = reportCalculator;
            _temporaryBindingFactory = temporaryBindingFactory;
            _failSignal = failSignal;
        }


        public override void Execute()
        {
            Log.TraceMessage();

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
                        .To(sensors.Cast<ExperimentSensor>().ToList());

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
            var ruleBuilder = _ruleBuilderProvider.GetBuilder(ruleConfig);

            if (!ruleBuilder.Any())
                throw new ArgumentException("No builder for " + ruleConfig.ToSafeString() + " available");

            return ruleBuilder.Value.With(b => b.Build(ruleConfig, _ruleBuilderProvider, _temporaryBindingFactory));
        }
    }
}
