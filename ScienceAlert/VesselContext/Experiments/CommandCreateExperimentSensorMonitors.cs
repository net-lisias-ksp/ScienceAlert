using System;
using System.Collections.Generic;
using System.Linq;
using strange.extensions.command.impl;

namespace ScienceAlert.VesselContext.Experiments
{
// ReSharper disable once ClassNeverInstantiated.Global
    public class CommandCreateExperimentSensorMonitors : Command
    {
        private readonly IEnumerable<ScienceExperiment> _experiments;
        private readonly IExperimentSensorMonitorFactory _sensorMonitorFactory;

        public CommandCreateExperimentSensorMonitors(
            IEnumerable<ScienceExperiment> experiments,
            IExperimentSensorMonitorFactory sensorMonitorFactory)
        {
            if (experiments == null) throw new ArgumentNullException("experiments");
            if (sensorMonitorFactory == null) throw new ArgumentNullException("sensorMonitorFactory");
            _experiments = experiments;
            _sensorMonitorFactory = sensorMonitorFactory;
        }


        public override void Execute()
        {
            var monitors = _experiments.Select(exp => _sensorMonitorFactory.Create(exp)).ToList();

            injectionBinder.Bind<IEnumerable<IExperimentSensorMonitor>>()
                .Bind<List<IExperimentSensorMonitor>>()
                .ToValue(monitors);
        }
    }
}
