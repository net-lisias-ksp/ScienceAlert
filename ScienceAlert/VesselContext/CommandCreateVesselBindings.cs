using System;
using JetBrains.Annotations;
using strange.extensions.command.impl;
using ScienceAlert.Game;

namespace ScienceAlert.VesselContext
{

// ReSharper disable once ClassNeverInstantiated.Global
    class CommandCreateVesselBindings : Command
    {
        private readonly IGameFactory _gameFactory;
        private readonly SignalActiveVesselModified _activeVesselModified;
        private readonly SignalActiveVesselCrewModified _activeVesselCrewModified;
        private readonly SignalDominantBodyChanged _dominantBodyChanged;

        public CommandCreateVesselBindings(
            IGameFactory gameFactory,
            SignalActiveVesselModified activeVesselModified,
            SignalActiveVesselCrewModified activeVesselCrewModified,
            SignalDominantBodyChanged dominantBodyChanged)
        {
            if (gameFactory == null) throw new ArgumentNullException("gameFactory");
            if (activeVesselModified == null) throw new ArgumentNullException("activeVesselModified");
            if (activeVesselCrewModified == null) throw new ArgumentNullException("activeVesselCrewModified");
            if (dominantBodyChanged == null) throw new ArgumentNullException("dominantBodyChanged");

            _gameFactory = gameFactory;
            _activeVesselModified = activeVesselModified;
            _activeVesselCrewModified = activeVesselCrewModified;
            _dominantBodyChanged = dominantBodyChanged;
        }


        public override void Execute()
        {
            var vessel = new KspVessel(_gameFactory, FlightGlobals.ActiveVessel);

            _activeVesselModified.AddListener(vessel.OnVesselModified);
            _activeVesselCrewModified.AddListener(vessel.OnVesselCrewModified);
            _dominantBodyChanged.AddListener((args) => vessel.OnDominantBodyChanged());

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
