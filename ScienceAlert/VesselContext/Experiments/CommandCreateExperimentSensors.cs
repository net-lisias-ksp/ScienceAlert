using System;
using System.Collections.Generic;
using System.Linq;
using strange.extensions.command.impl;
using strange.extensions.context.api;
using UnityEngine;

namespace ScienceAlert.VesselContext.Experiments
{
// ReSharper disable once ClassNeverInstantiated.Global
    public class CommandCreateExperimentSensors : Command
    {
        private readonly GameObject _vesselContextView;
        private readonly IEnumerable<ScienceExperiment> _experiments;
        private readonly IExperimentSensorFactory _sensorFactory;


        public CommandCreateExperimentSensors(
            [Name(ContextKeys.CONTEXT_VIEW)] GameObject vesselContextView,
            IEnumerable<ScienceExperiment> experiments,
            IExperimentSensorFactory sensorFactory)
        {
            if (vesselContextView == null) throw new ArgumentNullException("vesselContextView");
            if (experiments == null) throw new ArgumentNullException("experiments");
            if (sensorFactory == null) throw new ArgumentNullException("sensorFactory");

            _vesselContextView = vesselContextView;
            _experiments = experiments;
            _sensorFactory = sensorFactory;
        }


        public override void Execute()
        {
            Log.Verbose("Creating experiment sensors");

            var sensorList = _experiments.Select(experiment => _sensorFactory.Create(experiment)).ToList();

            injectionBinder.Bind<List<ExperimentSensor>>().ToValue(sensorList);

            var updater = _vesselContextView.AddComponent<ExperimentSensorUpdater>();

            injectionBinder.injector.Inject(updater, false);

            injectionBinder.Unbind<List<ExperimentSensor>>();

            Log.Verbose("Created experiment sensors");
        }
    }
}
