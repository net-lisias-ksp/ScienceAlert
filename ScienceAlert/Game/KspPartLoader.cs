using System;
using JetBrains.Annotations;
using ReeperCommon.Containers;

namespace ScienceAlert.Game
{
    class KspPartLoader : IPartLoader
    {
        private readonly IGameFactory _factory;

        public KspPartLoader([NotNull] IGameFactory factory)
        {
            if (factory == null) throw new ArgumentNullException("factory");
            _factory = factory;
        }


        public Maybe<IPart> GetPartByName([NotNull] string name)
        {
            if (name == null) throw new ArgumentNullException("name");

            return PartLoader.getPartInfoByName(name)
                .With(ap => _factory.Create(ap.partPrefab))
                .ToMaybe();
        }
    }
}
