using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using JetBrains.Annotations;
using ReeperCommon.Containers;
using ReeperCommon.Logging;
using strange.extensions.command.impl;
using strange.extensions.injector.api;
using ScienceAlert.Game;

namespace ScienceAlert.Core
{
    class CommandCreateExperimentConfigurations : Command
    {
        private readonly IGameDatabase _database;
        private readonly string _experimentConfigurationNodeName;
        private readonly IObjectFromConfigNodeBuilder<ExperimentConfiguration, ConfigNode, IInjectionBinder> _configurationBuilder;

        private readonly Lazy<List<IUrlConfig>> _definedConfigurations;

 
        private class ScienceExperimentComparer : IEqualityComparer<ScienceExperiment>
        {
            public bool Equals(ScienceExperiment x, ScienceExperiment y)
            {
                return x == y || x.Return(e => e.id, string.Empty) == y.Return(e => e.id, string.Empty);
            }

            public int GetHashCode(ScienceExperiment obj)
            {
                return obj.Return(e => e.id.GetHashCode(), 0);
            }
        }


        public CommandCreateExperimentConfigurations([NotNull] IGameDatabase database,
            [NotNull, Name(CoreContextKeys.ExperimentConfigurationNodeName)] string experimentConfigurationNodeName,
            [NotNull] IObjectFromConfigNodeBuilder<ExperimentConfiguration, ConfigNode, IInjectionBinder> configurationBuilder)
        {
            if (database == null) throw new ArgumentNullException("database");
            if (experimentConfigurationNodeName == null)
                throw new ArgumentNullException("experimentConfigurationNodeName");
            if (configurationBuilder == null) throw new ArgumentNullException("configurationBuilder");

            _database = database;
            _experimentConfigurationNodeName = experimentConfigurationNodeName;
            _configurationBuilder = configurationBuilder;
            _definedConfigurations = new Lazy<List<IUrlConfig>>(GetCustomExperimentConfigurationConfigNodes);
        }


        public override void Execute()
        {
            var configurations = CreateConfigurations();

            Log.Verbose("Created " + configurations.Count + " configurations");

            injectionBinder.Bind<ReadOnlyCollection<ExperimentConfiguration>>()
                .To(configurations.AsReadOnly())
                .CrossContext();
        }


        // Postcondition: no duplicate experiments in list
        private List<ExperimentConfiguration> CreateConfigurations()
        {
            Log.Debug("Creating experiment configurations");

            var configurations =
                new Dictionary<ScienceExperiment, ExperimentConfiguration>(new ScienceExperimentComparer());

            foreach (var raw in _definedConfigurations.Value)
            {
                try
                {
                    var config = LoadConfiguration(raw);

                    if (!config.Any())
                    {
                        Log.Warning("Failed to load " + _experimentConfigurationNodeName + " from " + raw.Url);
                        continue;
                    }

                    if (configurations.ContainsKey(config.Value.Experiment))
                    {
                        Log.Error("Duplicate experiment configuration defined for " + config.Value.Experiment.id +
                                  " at " + raw.Url);
                        continue;
                    }

                    configurations.Add(config.Value.Experiment, config.Value);
                    Log.Verbose("Added custom configuration for " + config.Value.Experiment.id + " from " + raw.Url);
                }
                catch (Exception e)
                {
                    Log.Error("Failed to create experiment configuration from " + raw.Url);
                    Log.Error("Exception was: " + e);
                }
            }

            foreach (var exp in ResearchAndDevelopment.GetExperimentIDs().Select(ResearchAndDevelopment.GetExperiment))
                if (!configurations.ContainsKey(exp))
                {
                    configurations.Add(exp, CreateDefaultConfiguration(exp));
                    Log.Verbose("Added default experiment configuration for " + exp.id);
                }

            return configurations.Values.ToList();
        }


        private Maybe<ExperimentConfiguration> LoadConfiguration(IUrlConfig raw)
        {
            if (raw == null) return Maybe<ExperimentConfiguration>.None;

            var config = raw.Config;

            if (_configurationBuilder.CanHandle(config, injectionBinder))
                return _configurationBuilder.Build(config, injectionBinder).ToMaybe();

            Log.Error("ExperimentConfiguration at " + raw.Url + " seems to be invalid");
            return Maybe<ExperimentConfiguration>.None;
        }


        private static ExperimentConfiguration CreateDefaultConfiguration([NotNull] ScienceExperiment experiment)
        {
            if (experiment == null) throw new ArgumentNullException("experiment");

            return new ExperimentConfiguration(experiment, Maybe<ConfigNode>.None, Maybe<string>.None);
        }


        private List<IUrlConfig> GetCustomExperimentConfigurationConfigNodes()
        {
            return _database.GetConfigs(_experimentConfigurationNodeName).ToList();
        }
    }
}
