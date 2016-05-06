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
            [Name(CoreKeys.CoreContextView)] GameObject gameContext,
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

            yield return 0; // wait for view to start

            Release();
        }
    }
}
