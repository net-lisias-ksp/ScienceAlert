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
            var allDefinitionConfigs = _gameDatabase.GetConfigs(SensorDefinitionBuilder.SensorDefinitionNodeName).ToList();
            var validConfigs = allDefinitionConfigs.Where(uc => _sensorDefinitionBuilder.CanHandle(uc.Config)).ToList();

            LogUnhandledConfigs(allDefinitionConfigs.Except(validConfigs));

            var customDefinitions = new List<SensorDefinition>();

            foreach (var urlConfig in validConfigs)
            {
                Log.Debug("Building custom definition from " + urlConfig.Url);

                try
                {
                    var definition = _sensorDefinitionBuilder.Build(urlConfig.Config);

                    if (definition == null)
                        throw new InvalidOperationException("Sensor definition builder returned a null definition!");

                    Log.Verbose("Using custom definition for " + definition.Experiment.id + " from " + urlConfig.Url);
                    customDefinitions.Add(definition);
                }
                catch (Exception e)
                {
                    Log.Error("Error while creating " + urlConfig.Url + ": " + e);
                }
            }

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
