using System;
using ScienceAlert.Core;
using strange.extensions.command.impl;
using strange.extensions.context.api;
using strange.extensions.injector;
using UnityEngine;

namespace ScienceAlert
{
// ReSharper disable once ClassNeverInstantiated.Global
    public class CommandConfigureGameEvents : Command
    {
        private const string GameContextViewName = "ScienceAlert.GameContext";

        private readonly GameObject _contextView;

        public CommandConfigureGameEvents([Name(ContextKeys.CONTEXT_VIEW)] GameObject contextView)
        {
            if (contextView == null) throw new ArgumentNullException("contextView");
            _contextView = contextView;
        }


        public override void Execute()
        {
            SetupBindings();
            CreateViews();
            MapCommands();
        }


        private void SetupBindings()
        {


            
        }


        private void CreateViews()
        {
            var gameContext = new GameObject(GameContextViewName);
            gameContext.transform.parent = _contextView.transform;

            injectionBinder.Bind<GameObject>().To(gameContext).ToName(CoreKeys.GameContextView).CrossContext();

            gameContext.AddComponent<GameEventView>();
        }


        private void MapCommands()
        {
            //commandBinder.Bind<SignalVesselDestroyed>()
            //    .To<CommandDestroyActiveVesselView>();

            //commandBinder.Bind<SignalActiveVesselChanged>()
            //    .To<CommandDestroyActiveVesselView>()
            //    .To<CommandCreateActiveVesselView>();
        }
    }
}
