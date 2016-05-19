using System;
using System.Linq;
using ReeperCommon.Containers;
using ReeperCommon.Logging;
using UnityEngine;

namespace ScienceAlert.Game
{
    // note: make sure this isn't in the cross context; it uses IGameFactory which uses non-cross-context signals
    public class KspGameDatabase : IGameDatabase
    {
        private readonly IGameFactory _gameFactory;

        public KspGameDatabase(IGameFactory gameFactory)
        {
            if (gameFactory == null) throw new ArgumentNullException("gameFactory");
            _gameFactory = gameFactory;
        }


        public IUrlConfig[] GetConfigs(string nodeName)
        {
            if (string.IsNullOrEmpty(nodeName))
                throw new ArgumentException("Must specify node name", "nodeName");

            var timeStart = Time.realtimeSinceStartup;

            try
            {
                return GameDatabase.Instance.GetConfigs(nodeName)
                    .Select(c => _gameFactory.Create(c))
                    .ToArray();


            }
            finally
            {
                Log.Performance("KspGameDatabase.GetConfigs [ " + nodeName + "] time: " +
                                (Time.realtimeSinceStartup - timeStart).ToString("F3"));
            }
        }


        public Maybe<AudioClip> GetAudioClip(string clipUrl)
        {
            if (string.IsNullOrEmpty(clipUrl)) throw new ArgumentException("Must specify a valid value", "clipUrl");

            return GameDatabase.Instance.GetAudioClip(clipUrl).ToMaybe();
        }
    }
}
