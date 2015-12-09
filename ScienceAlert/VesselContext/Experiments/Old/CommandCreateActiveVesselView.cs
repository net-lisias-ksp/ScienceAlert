//using System;
//using ReeperCommon.Containers;
//using ReeperCommon.Logging;
//using ScienceAlert.Core;
//using strange.extensions.command.impl;
//using strange.extensions.injector;
//using UnityEngine;

//namespace ScienceAlert.Experiments
//{
//    // ReSharper disable once UnusedMember.Global
//// ReSharper disable once ClassNeverInstantiated.Global
//    public class CommandCreateActiveVesselView : Command
//    {
//        private readonly GameObject _gameContext;
//        private readonly ILog _log;

//        public CommandCreateActiveVesselView([Name(CoreKeys.GameContextView)] GameObject gameContext, ILog log)
//        {
//            if (gameContext == null) throw new ArgumentNullException("gameContext");
//            if (log == null) throw new ArgumentNullException("log");

//            _gameContext = gameContext;
//            _log = log;
//        }


//        public override void Execute()
//        {
//            // only create a vessel if, you know, there's an actual vessel to serve as a view ...
//            FlightGlobals.ActiveVessel
//                .IfNull(() => _log.Warning("No active vessel; view not created"))
//                .Do(activeVessel =>
//                {
//                    injectionBinder.GetBinding<Vessel>().Do(b => injectionBinder.Unbind<Vessel>());
//                    injectionBinder.Bind<Vessel>().To(activeVessel).CrossContext();

//                    _gameContext.AddComponent<ActiveVesselView>();
//                });
//        }
//    }
//}
