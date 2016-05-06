using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ReeperCommon.Containers;
using ReeperCommon.Logging;
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
                var config = GetConfigNodeFromDatabase().Or(GetConfigNodeFromDisk()); // prefer the cached version whenever possible to reduce IO time

                if (!config.Any())
                    Log.Warning("No saved shared configuration data found; using default values");
                else LoadConfigurationFrom(config.Single());

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


        protected Maybe<ConfigNode> GetConfigNodeFromDatabase()
        {
            return GameDatabase.Instance.GetConfigNodes(SharedConfiguration.NodeName)
                .With(nodes =>
                {
                    if (nodes.Length > 1)
                    {
                        Log.Warning(
                            "Multiple ConfigNodes for shared configuration found. Have you installed ScienceAlert in two places or moved any files?");

                        foreach (var n in GetDuplicateNodes(SharedConfiguration.NodeName))
                            Log.Warning("Duplicate ConfigNode url: " + n.Url);

                        return null; // don't know which one to use so we won't use any
                    }

                    return nodes.SingleOrDefault();
                })
                .ToMaybe();
        }


        private Maybe<ConfigNode> GetConfigNodeFromDisk()
        {
            var sharedConfigPath = ConfigPathProviderQuery.GetFullPath();

            if (!File.Exists(sharedConfigPath))
                return Maybe<ConfigNode>.None;

            try
            {
                return ConfigNode.Load(sharedConfigPath).If(n => n.HasData).ToMaybe();
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
