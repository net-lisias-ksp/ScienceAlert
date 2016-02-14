﻿using System;
using System.Collections.Generic;
using System.Linq;
using ReeperCommon.Serialization;
using ScienceAlert.Game;
using ScienceAlert.VesselContext.Experiments.Rules;
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
        private readonly SignalCriticalShutdown _seriousProblemSignal;

        public CommandCreateExperimentSensors(
            [Name(ContextKeys.CONTEXT_VIEW)] GameObject vesselContextView,
            IConfigNodeSerializer serializer,
            IEnumerable<SensorDefinition> sensorDefinitions,
            IScienceSubjectProvider subjectProvider,
            IExperimentReportValueCalculator reportCalculator,
            SignalCriticalShutdown seriousProblemSignal)
        {
            if (vesselContextView == null) throw new ArgumentNullException("vesselContextView");
            if (serializer == null) throw new ArgumentNullException("serializer");
            if (sensorDefinitions == null) throw new ArgumentNullException("sensorDefinitions");
            if (subjectProvider == null) throw new ArgumentNullException("subjectProvider");
            if (reportCalculator == null) throw new ArgumentNullException("reportCalculator");
            if (seriousProblemSignal == null) throw new ArgumentNullException("seriousProblemSignal");

            _vesselContextView = vesselContextView;
            _serializer = serializer;
            _sensorDefinitions = sensorDefinitions;
            _subjectProvider = subjectProvider;
            _reportCalculator = reportCalculator;
            _seriousProblemSignal = seriousProblemSignal;
        }


        public override void Execute()
        {
            var sensors = CreateSensors();

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
            }

            Log.Debug("Created " + sensors.Count + " of " + _sensorDefinitions.Count() + " successfully");
            return sensors;
        }

        private void CreateExperimentSensorUpdater()
        {
            var updater = _vesselContextView.AddComponent<ExperimentSensorUpdater>();

            injectionBinder.injector.Inject(updater, false);
        }
    }
}
