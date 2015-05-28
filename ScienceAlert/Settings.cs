using ReeperCommon.Logging;
using UnityEngine;


namespace ScienceAlert
{
    // ReSharper disable ConvertToConstant.Local
    // ReSharper disable FieldCanBeMadeReadOnly.Local
    public class Settings : IConfigNode
    {
        [Persistent] public LogLevel LogLevel = LogLevel.Debug;



        public void Load(ConfigNode node) { ConfigNode.LoadObjectFromConfig(this, node);}
        public void Save(ConfigNode node) { ConfigNode.CreateConfigFromObject(this, node); }
    }
}
