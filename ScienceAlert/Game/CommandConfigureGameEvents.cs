using System;
using strange.extensions.command.impl;
using strange.extensions.context.api;
using UnityEngine;

namespace ScienceAlert.Game
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
            CreateViews();
        }


        private void CreateViews()
        {
            var gameContext = new GameObject(GameContextViewName);
            gameContext.transform.parent = _contextView.transform;

            gameContext.AddComponent<GameEventView>();
        }
    }
}
