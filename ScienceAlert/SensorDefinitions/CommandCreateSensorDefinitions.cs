using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using ReeperCommon.Logging;
using strange.extensions.command.impl;
using ScienceAlert.Game;

namespace ScienceAlert.SensorDefinitions
{
// ReSharper disable once ClassNeverInstantiated.Global
    class CommandCreateSensorDefinitions : Command
    {
        private const string SensorDefinitionNodeName = "SA_SENSOR_DEFINITION";

        private readonly IGameDatabase _gameDatabase;
        private readonly IConfigNodeObjectBuilder<SensorDefinition> _sensorDefinitionBuilder;
        private readonly ISensorDefinitionFactory _factory;
        private readonly ReadOnlyCollection<ScienceExperiment> _experiments;


        public CommandCreateSensorDefinitions(
            IGameDatabase gameDatabase, 
            IConfigNodeObjectBuilder<SensorDefinition> sensorDefinitionBuilder,
            ISensorDefinitionFactory factory,
            ReadOnlyCollection<ScienceExperiment> experiments)
        {
            if (gameDatabase == null) throw new ArgumentNullException("gameDatabase");
            if (sensorDefinitionBuilder == null) throw new ArgumentNullException("sensorDefinitionBuilder");
            if (factory == null) throw new ArgumentNullException("factory");
            if (experiments == null) throw new ArgumentNullException("experiments");

            _gameDatabase = gameDatabase;
            _sensorDefinitionBuilder = sensorDefinitionBuilder;
            _factory = factory;
            _experiments = experiments;
        }


        public override void Execute()
        {
            Log.Verbose("Creating sensor definitions");

            var customDefinitions = CreateCustomDefinitions();
            var defaultDefinitions =
                CreateDefaultDefinitions(_experiments.Where(e => customDefinitions.All(cd => cd.Experiment.id != e.id)));

            var allDefinitions = customDefinitions.Union(defaultDefinitions).ToList().AsReadOnly();

            Log.Verbose("Created " + allDefinitions.Count + " sensor definitions");

            injectionBinder
                .Bind<ReadOnlyCollection<SensorDefinition>>()
                    .To(allDefinitions).CrossContext();
        }


        private IEnumerable<SensorDefinition> CreateCustomDefinitions()
        {
            var allDefinitionConfigs = _gameDatabase.GetConfigs(SensorDefinitionNodeName).ToList();

            LogUnhandledConfigs(allDefinitionConfigs
                .Where(uc => !_sensorDefinitionBuilder.CanHandle(uc.Config)));

            var customDefinitions = 
                allDefinitionConfigs.Where(uc => _sensorDefinitionBuilder.CanHandle(uc.Config))
                    .Select(uc => _sensorDefinitionBuilder.Build(uc.Config))
                    .ToList();

            customDefinitions.ForEach(cd => Log.Verbose("Using custom definition for: " + cd.Experiment.id));

            return customDefinitions;
        }


        private IEnumerable<SensorDefinition> CreateDefaultDefinitions(IEnumerable<ScienceExperiment> experiments)
        {
            if (experiments == null) throw new ArgumentNullException("experiments");

            var defaultDefinitions = experiments.Select(e => _factory.CreateDefault(e)).ToList();

            defaultDefinitions.ForEach(sd => Log.Verbose("Using default definition for: " + sd.Experiment.id));

            return defaultDefinitions;
        }


        private static void LogUnhandledConfigs(IEnumerable<IUrlConfig> configs)
        {
            foreach (var c in configs)
                Log.Verbose("Sensor definition skipped: " + c.Url);
        }
    }
}
