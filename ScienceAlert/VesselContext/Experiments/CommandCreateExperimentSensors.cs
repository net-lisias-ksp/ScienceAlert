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
        private readonly IExistingScienceSubjectProvider _subjectProvider;
        private readonly IExperimentReportValueCalculator _reportCalculator;
        private readonly ITemporaryBindingFactory _temporaryBindingFactory;
        private readonly ICriticalShutdownEvent _failSignal;

        [Inject] public IVessel ActiveVessel { get; set; }

        public CommandCreateExperimentSensors(
            IRuleFactory ruleFactory,
            ReadOnlyCollection<SensorDefinition> sensorDefinitions,
            IExistingScienceSubjectProvider subjectProvider,
            IExperimentReportValueCalculator reportCalculator,
            ITemporaryBindingFactory temporaryBindingFactory,
            ICriticalShutdownEvent failSignal)
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
            var rawSensors = new List<ExperimentSensor>();

            try
            {
                rawSensors = CreateSensors().ToList();

                if (!rawSensors.Any())
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
                var sensors = rawSensors.Cast<IExperimentSensor>().ToList();

                injectionBinder.Bind<ReadOnlyCollection<IExperimentSensor>>().To(sensors.AsReadOnly());

                Log.Verbose("Created " + rawSensors.Count + " experiment sensors");
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

            var sensor = new ExperimentSensor(definition.Experiment,
                    _subjectProvider,
                    _reportCalculator,
                    ActiveVessel,
                    ActiveVessel,
                    ActiveVessel,
                    CreateRule(definition.OnboardRuleDefinition),
                    CreateRule(definition.AvailabilityRuleDefinition),
                    CreateRule(definition.ConditionRuleDefinition),
                    _subjectProvider.GetExistingSubject(definition.Experiment, ActiveVessel.ExperimentSituation, ActiveVessel.OrbitingBody, ActiveVessel.Biome));

            sensor.UpdateSensorValues(); // otherwise the sensor might have an invalid subject

            return sensor;
        }


        private ISensorRule CreateRule(ConfigNode ruleConfig)
        {
            if (ruleConfig == null) throw new ArgumentNullException("ruleConfig");

            if (!_ruleFactory.CanHandle(ruleConfig))
                throw new ArgumentException("No builder for " + ruleConfig.ToSafeString());


            return _ruleFactory.With(b => b.Build(ruleConfig, _ruleFactory, injectionBinder, _temporaryBindingFactory));
        }
    }
}
