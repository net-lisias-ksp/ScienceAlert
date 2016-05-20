using System;
using System.Collections;
using ReeperCommon.Logging;
using ReeperCommon.Utilities;
using strange.extensions.command.impl;
using UnityEngine;

namespace ScienceAlert.Core.Gui
{
// ReSharper disable once ClassNeverInstantiated.Global
    class CommandCreateAppLauncherView : Command
    {
        private readonly GameObject _gameContext;
        private readonly CoroutineHoster _coroutineRunner;

        public CommandCreateAppLauncherView(
            [Name(CrossContextKeys.CoreContextView)] GameObject gameContext,
            CoroutineHoster coroutineRunner)
        {
            if (gameContext == null) throw new ArgumentNullException("gameContext");
            if (coroutineRunner == null) throw new ArgumentNullException("coroutineRunner");

            _gameContext = gameContext;
            _coroutineRunner = coroutineRunner;
        }


        public override void Execute()
        {
            Retain();
            _coroutineRunner.StartCoroutine(CreateView());
        }


        private IEnumerator CreateView()
        {
            Log.Debug("Creating ApplicationLauncher view");
            _gameContext.AddComponent<ApplicationLauncherView>();

            // note to self: no critical shutdown subscriber here, since the view is attached to the core context GO and so will be
            // destroyed by the core context handler if something happens

            yield return 0; // wait for view to start

            Release();
        }
    }
}
