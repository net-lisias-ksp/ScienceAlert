using System;
using strange.extensions.command.impl;
using ScienceAlert.Game;

namespace ScienceAlert.VesselContext
{

// ReSharper disable once ClassNeverInstantiated.Global
    class CommandCreateVesselBindings : Command
    {
        private readonly IGameFactory _gameFactory;
        private readonly SignalActiveVesselModified _activeVesselModified;

        public CommandCreateVesselBindings(
            IGameFactory gameFactory,
            SignalActiveVesselModified activeVesselModified)
        {
            if (gameFactory == null) throw new ArgumentNullException("gameFactory");
            if (activeVesselModified == null) throw new ArgumentNullException("activeVesselModified");
            _gameFactory = gameFactory;
            _activeVesselModified = activeVesselModified;
        }


        public override void Execute()
        {
            var vessel = new KspVessel(_gameFactory, FlightGlobals.ActiveVessel);

            _activeVesselModified.AddListener(vessel.OnVesselModified);

            vessel.Rescan();

            // todo: crew transfer, eva events

            injectionBinder
                .Bind<IVessel>()
                .Bind<ICelestialBodyProvider>()
                .Bind<IExperimentSituationProvider>()
                .Bind<IExperimentBiomeProvider>()
                .Bind<IScienceContainerCollectionProvider>().To(vessel);
        }
    }
}
