using System;
using System.IO;
using ReeperKSP.FileSystem;
using ReeperKSP.Serialization;
using UnityEngine;

namespace ScienceAlert
{
    /// <summary>
    /// Shared configuration between all saves
    /// </summary>
// ReSharper disable once ClassNeverInstantiated.Global
    public class SharedConfiguration : ISharedConfigurationFilePathProvider // todo: separate interface for window configurations mapped to this object
    {
        public const string NodeName = "ScienceAlertSharedConfiguration";
        public const string FileName = "ScienceAlert.cfg";
        public const string FolderName = "PluginData";
        public const string ExperimentWindowConfigNodeName = "ExperimentWindow";

        [ReeperPersistent] public float ButtonFramerate = 15f;
        [ReeperPersistent] public readonly ConfigNode SoundConfig = new ConfigNode("Sounds");
        [ReeperPersistent] public readonly ConfigNode ExperimentWindowConfig = new ConfigNode(ExperimentWindowConfigNodeName);

        private const float FramerateMin = 1f;
        private const float FramerateMax = 60f;

        private readonly IDirectory _pluginDirectory;


        public SharedConfiguration(IDirectory pluginDirectory)
        {
            if (pluginDirectory == null) throw new ArgumentNullException("pluginDirectory");
            _pluginDirectory = pluginDirectory;
        }


        public float Framerate
        {
            get { return ButtonFramerate; }
            set { ButtonFramerate = Mathf.Clamp(value, FramerateMin, FramerateMax); }
        }


        public string GetFullPath()
        {
            return Path.Combine(Path.Combine(_pluginDirectory.FullPath, FolderName), FileName);
        }
    }
}
