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
//    public class CommandDestroyActiveVesselView : Command
//    {
//        private readonly GameObject _gameContext;
//        private readonly ILog _log;

//        public CommandDestroyActiveVesselView([Name(Keys.GameContextView)] GameObject gameContext, ILog log)
//        {
//            if (gameContext == null) throw new ArgumentNullException("gameContext");
//            if (log == null) throw new ArgumentNullException("log");

//            _gameContext = gameContext;
//            _log = log;
//        }

//        public override void Execute()
//        {
//            _gameContext.GetComponent<ActiveVesselView>()
//                .Do(v => _log.Verbose("Destroying vessel view"))
//                .Do(v => injectionBinder.Unbind<Vessel>())
//                .Do(UnityEngine.Object.Destroy)
//                .IfNull(() => _log.Debug("No vessel view to be destroyed"));
//        }
//    }
//}
