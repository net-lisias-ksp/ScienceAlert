using System;
using System.IO;
using ReeperCommon.FileSystem;
using ReeperCommon.Serialization;
using ScienceAlert.Gui;
using UnityEngine;

namespace ScienceAlert
{
    /// <summary>
    /// Shared configuration between all saves
    /// </summary>
// ReSharper disable once ClassNeverInstantiated.Global
    public class SharedConfiguration : IGuiConfiguration, ISharedConfigurationFilePathProvider
    {
        public const string NodeName = "ScienceAlertSharedConfiguration";
        public const string Filename = "ScienceAlert.cfg";

        [ReeperPersistent] public float ButtonFramerate = 15f;
        [ReeperPersistent] public readonly ConfigNode ExperimentViewConfig = new ConfigNode("ExperimentListView");
        [ReeperPersistent] public readonly ConfigNode VesselDebugViewConfig = new ConfigNode("VesselDebugView");

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
            return Path.Combine(_pluginDirectory.FullPath, Filename);
        }
    }
}
