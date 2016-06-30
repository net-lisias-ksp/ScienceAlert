using System;
using System.IO;
using System.Linq;
using ReeperCommon.Containers;
using ReeperCommon.Extensions;
using ReeperCommon.Logging;
using ReeperKSP.Extensions;
using ReeperKSP.FileSystem;
using ReeperKSP.Serialization;

namespace ScienceAlert.Core
{
// ReSharper disable once ClassNeverInstantiated.Global
    class CommandSaveSharedConfiguration : CommandLoadSharedConfiguration
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

                var destinationConfig = Serializer.CreateConfigNodeFromObject(SharedConfiguration);

                if (destinationConfig.IsNull() || !destinationConfig.HasData)
                    Log.Error("Failed to create ConfigNode from shared configuration");
                else
                {
                    SaveConfigToDisk(destinationConfig);
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
    }
}
