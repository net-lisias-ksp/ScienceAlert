using ReeperCommon.Containers;
using UnityEngine;

namespace ScienceAlert.Game
{
    public interface IGameDatabase
    {
        IUrlConfig[] GetConfigs(string nodeName);
        Maybe<AudioClip> GetAudioClip(string clipUrl);
    }
}
