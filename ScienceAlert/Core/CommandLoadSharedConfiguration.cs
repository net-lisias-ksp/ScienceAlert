using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ReeperCommon.Containers;
using ReeperCommon.Logging;
using ReeperKSP.Extensions;
using ReeperKSP.FileSystem;
using ReeperKSP.Serialization;
using strange.extensions.command.impl;

namespace ScienceAlert.Core
{
// ReSharper disable once UnusedMember.Global
    class CommandLoadSharedConfiguration : Command
    {
        protected readonly IDirectory PluginDirectory;
        protected readonly ISharedConfigurationFilePathProvider ConfigPathProviderQuery;
        protected readonly IConfigNodeSerializer Serializer;
        protected SharedConfiguration SharedConfiguration;

        public CommandLoadSharedConfiguration(
            IDirectory pluginDirectory,
            ISharedConfigurationFilePathProvider configPathProviderQuery,
            IConfigNodeSerializer serializer,
            SharedConfiguration sharedConfiguration)
        {
            if (pluginDirectory == null) throw new ArgumentNullException("pluginDirectory");
            if (configPathProviderQuery == null) throw new ArgumentNullException("configPathProviderQuery");
            if (serializer == null) throw new ArgumentNullException("serializer");
            if (sharedConfiguration == null) throw new ArgumentNullException("sharedConfiguration");

            PluginDirectory = pluginDirectory;
            ConfigPathProviderQuery = configPathProviderQuery;
            Serializer = serializer;
            SharedConfiguration = sharedConfiguration;
        }


        public override void Execute()
        {
            Log.Normal("Loading shared configuration...");

            try
            {
                var config = GetConfigNodeFromDisk(); 

                if (!config.Any())
                    Log.Warning("No saved shared configuration data found; using default values");
                else LoadConfigurationFrom(config.Single());

                injectionBinder.Bind<ConfigNode>()
                    .To(SharedConfiguration.ExperimentWindowConfig)
                    .ToName(CrossContextKeys.ExperimentWindowConfig)
                    .ToInject(false)
                    .CrossContext();

                Log.Warning("OptionsWindow config: " + SharedConfiguration.ExperimentWindowConfig.ToSafeString());
                Log.Warning("Which was loaded from: " + config.Value.ToSafeString());

                Log.Normal("Successfully loaded shared configuration");
            }
            catch (Exception e)
            {
                Log.Error("An exception occurred: " + e);
                Log.Warning("Shared configuration is now in an uncertain state");
            }
        }


        private void LoadConfigurationFrom(ConfigNode node)
        {
            if (node == null) throw new ArgumentNullException("node");

            Serializer.LoadObjectFromConfigNode(ref SharedConfiguration, node);
        }


        private Maybe<ConfigNode> GetConfigNodeFromDisk()
        {
            var sharedConfigPath = ConfigPathProviderQuery.GetFullPath();

            if (!File.Exists(sharedConfigPath))
                return Maybe<ConfigNode>.None;

            try
            {
                // the whole contents of the ConfigNode gets wrapped in a root node. There should be exactly one subnode in it
                // or else somebody has screwed with our ConfigNode and who knows whether it's useful?
                return ConfigNode.Load(sharedConfigPath).If(n => n.HasData).If(n => n.CountNodes == 1).With(n => n.nodes[0]).ToMaybe();
            }
            catch (Exception e)
            {
                Log.Error("Error while retrieving shared configuration from disk: " + e);
                return Maybe<ConfigNode>.None;
            }
        }


        private IEnumerable<IFile> GetDuplicateNodes(string name)
        {
            var parent = PluginDirectory;

            while (parent.Parent.Any())
                parent = parent.Parent.Value;

            return parent.RecursiveFiles()
                .Where(f => f.UrlFile.file.ContainsConfig(name));
        }
    }
}
