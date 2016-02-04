using System;
using System.Linq;

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

            return GameDatabase.Instance.GetConfigs(nodeName)
                .Select(c => _gameFactory.Create(c))
                .ToArray();
        }
    }
}
