using System;
using System.Linq;
using UnityEngine;

namespace ScienceAlert.Game
{
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
    }
}
