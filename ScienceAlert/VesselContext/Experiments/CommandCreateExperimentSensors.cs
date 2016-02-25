using System;
using System.Collections.Generic;
using System.Linq;
using ReeperCommon.Logging;
using ReeperCommon.Serialization;
using ScienceAlert.Core;
using ScienceAlert.Game;
using strange.extensions.command.impl;
using strange.extensions.context.api;
using UnityEngine;

namespace ScienceAlert.VesselContext.Experiments
{
// ReSharper disable once ClassNeverInstantiated.Global
    public class CommandCreateExperimentSensors : Command
    {
        private readonly GameObject _vesselContextView;
        private readonly IConfigNodeSerializer _serializer;
        private readonly IEnumerable<SensorDefinition> _sensorDefinitions;
        private readonly IScienceSubjectProvider _subjectProvider;
        private readonly IExperimentReportValueCalculator _reportCalculator;
        private readonly SignalTriggerSensorStatusUpdate _triggerSignal;
        private readonly SignalCriticalShutdown _seriousProblemSignal;

        public CommandCreateExperimentSensors(
            [Name(ContextKeys.CONTEXT_VIEW)] GameObject vesselContextView,
            IConfigNodeSerializer serializer,
            IEnumerable<SensorDefinition> sensorDefinitions,
            IScienceSubjectProvider subjectProvider,
            IExperimentReportValueCalculator reportCalculator,
            SignalTriggerSensorStatusUpdate triggerSignal,
            SignalCriticalShutdown seriousProblemSignal)
        {
            if (vesselContextView == null) throw new ArgumentNullException("vesselContextView");
            if (serializer == null) throw new ArgumentNullException("serializer");
            if (sensorDefinitions == null) throw new ArgumentNullException("sensorDefinitions");
            if (subjectProvider == null) throw new ArgumentNullException("subjectProvider");
            if (reportCalculator == null) throw new ArgumentNullException("reportCalculator");
            if (triggerSignal == null) throw new ArgumentNullException("triggerSignal");
            if (seriousProblemSignal == null) throw new ArgumentNullException("seriousProblemSignal");

            _vesselContextView = vesselContextView;
            _serializer = serializer;
            _sensorDefinitions = sensorDefinitions;
            _subjectProvider = subjectProvider;
            _reportCalculator = reportCalculator;
            _triggerSignal = triggerSignal;
            _seriousProblemSignal = seriousProblemSignal;
        }


        public override void Execute()
        {
            var sensors = CreateSensors().ToList();

            if (!sensors.Any())
            {
                Log.Error("Failed to create any experiment sensors -- something is wrong");
                _seriousProblemSignal.Dispatch();
                Fail();
                return;
            }


            try
            {
                injectionBinder.Bind<List<ExperimentSensor>>().To(sensors);
                CreateExperimentSensorUpdater();
            }
            finally
            {
                injectionBinder.Unbind<List<ExperimentSensor>>();
                injectionBinder.Bind<IEnumerable<IExperimentSensor>>().To(sensors.Cast<IExperimentSensor>().ToList());
            }
            
            Log.Verbose("Created experiment sensors");
        }


        private IEnumerable<ExperimentSensor> CreateSensors()
        {
            var sensors = new List<ExperimentSensor>();

            
            foreach (var sensorDefinition in _sensorDefinitions)
            {
                try
                {
                    injectionBinder.Bind<ScienceExperiment>().To(sensorDefinition.Experiment);

                    var sensor = new ExperimentSensor(
                        sensorDefinition.Experiment,
                        _subjectProvider,
                        _reportCalculator,
                        sensorDefinition.OnboardRuleFactory.Create(injectionBinder, _serializer),
                        sensorDefinition.AvailabilityRuleFactory.Create(injectionBinder, _serializer),
                        sensorDefinition.ConditionRuleFactory.Create(injectionBinder, _serializer));

                    sensors.Add(sensor);
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

        private void CreateExperimentSensorUpdater()
        {
            var updater = _vesselContextView.AddComponent<ExperimentSensorUpdater>();

            injectionBinder.injector.Inject(updater, false);

            _triggerSignal.AddListener(updater.OnStatusUpdateRequested);
        }
    }
}
