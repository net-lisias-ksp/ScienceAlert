using System;
using System.IO;
using System.Linq;
using ReeperCommon.Containers;
using ReeperCommon.Extensions;
using ReeperCommon.FileSystem;
using ReeperCommon.Serialization;

namespace ScienceAlert.Core
{
// ReSharper disable once ClassNeverInstantiated.Global
    public class CommandSaveSharedConfiguration : CommandLoadSharedConfiguration
    {
        private readonly SignalSharedConfigurationSaving _savingSignal;

        public CommandSaveSharedConfiguration(
            IDirectory pluginDirectory, 
            ISharedConfigurationFilePathProvider configPathProviderQuery, 
            IConfigNodeSerializer serializer, 
            SharedConfiguration sharedConfiguration,
            SignalSharedConfigurationSaving savingSignal) : base(pluginDirectory, configPathProviderQuery, serializer, sharedConfiguration)
        {
            if (savingSignal == null) throw new ArgumentNullException("savingSignal");
            _savingSignal = savingSignal;
        }

        public override void Execute()
        {
            Log.Normal("Saving shared configuration...");

            try
            {
                _savingSignal.Dispatch();

                var databaseConfig = GetConfigNodeFromDatabase();
                var serialized = Serializer.CreateConfigNodeFromObject(base.SharedConfiguration);

                if (serialized.IsNull() || !serialized.HasData)
                    Log.Error("Failed to create ConfigNode from shared configuration");
                else
                {
                    // What's going on here? Well to avoid IO delays, we'll use the settings file already
                    // in memory (from GameDatabase) if possible. The problem is that should any of those settings
                    // change during the game session, that copy will be outdated. We can keep it up to date by
                    // deleting its original contents and copying the new data straight into the original
                    var destinationConfig = databaseConfig
                        .Do(db => db.ClearData())
                        .Do(serialized.CopyTo)
                        .SingleOrDefault() ?? serialized;

                    destinationConfig.name = SharedConfiguration.NodeName;

                    SaveConfigToDisk(destinationConfig);

                    // if GameDatabase doesn't already have a cached copy, let's make sure we include one
                    databaseConfig.IfNull(() => InsertConfigIntoDatabase(destinationConfig));
                }

                Log.Normal("Shared configuration saved");
            }
            catch (Exception e)
            {
                Log.Error("Failed: " + e);
                Log.Warning("Shared configuration may not have been saved");
            }
        }


        private void SaveConfigToDisk(ConfigNode node)
        {
            if (node == null) throw new ArgumentNullException("node");

            node.Write(ConfigPathProviderQuery.GetFullPath(), "ScienceAlert shared settings");
        }


        private void InsertConfigIntoDatabase(ConfigNode node)
        {
            var configPath = ConfigPathProviderQuery.GetFullPath();

            if (!File.Exists(configPath))
            {
                Log.Error("Cannot insert node " + node.name + " into GameDatabase because " + configPath +
                          " does not exist");
                return;
            }

            var configFile = new UrlDir.UrlFile(PluginDirectory.UrlDir.KspDir, new FileInfo(configPath));
            // note to self: do not AddConfig here; KSP reads the path off disk and loads the ConfigNode itself

            PluginDirectory.UrlDir.AddFile(new KSPUrlFile(configFile));
        }
    }
}
