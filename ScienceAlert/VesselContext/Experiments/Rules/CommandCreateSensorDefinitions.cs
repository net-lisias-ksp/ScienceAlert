using System;
using System.Collections.Generic;
using System.Linq;
using ScienceAlert.Game;
using strange.extensions.command.impl;

namespace ScienceAlert.VesselContext.Experiments.Rules
{
    public class CommandCreateSensorDefinitions : Command
    {
        private const string SensorDefinitionNodeName = "SA_SENSOR_DEFINITION";

        private readonly IGameDatabase _gameDatabase;
        private readonly IConfigNodeObjectGraphBuilder<SensorDefinition> _sensorDefinitionBuilder;


        public CommandCreateSensorDefinitions(
            IGameDatabase gameDatabase,
            IConfigNodeObjectGraphBuilder<SensorDefinition> sensorDefinitionBuilder)
        {
            if (gameDatabase == null) throw new ArgumentNullException("gameDatabase");
            if (sensorDefinitionBuilder == null) throw new ArgumentNullException("sensorDefinitionBuilder");

            _gameDatabase = gameDatabase;
            _sensorDefinitionBuilder = sensorDefinitionBuilder;
        }


        public override void Execute()
        {
            Log.Debug("Creating sensor definitions");

            var allDefinitionConfigs = _gameDatabase.GetConfigs(SensorDefinitionNodeName).ToList();

            var definitions =
                allDefinitionConfigs.Where(uc => _sensorDefinitionBuilder.CanHandle(uc.Config))
                    .Select(uc => _sensorDefinitionBuilder.Build(uc.Config))
                    .ToList();

            injectionBinder.Bind<IEnumerable<SensorDefinition>>()
                .ToValue(definitions).CrossContext();

            LogUnhandledConfigs(allDefinitionConfigs
                .Where(uc => !_sensorDefinitionBuilder.CanHandle(uc.Config)));
        }



        private void LogUnhandledConfigs(IEnumerable<IUrlConfig> configs)
        {
            foreach (var c in configs)
                Log.Verbose("Sensor definition skipped: " + c.Url);
        }
    }
}
