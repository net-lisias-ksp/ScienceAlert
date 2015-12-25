using System;
using System.Collections;
using strange.extensions.command.impl;
using strange.extensions.injector;
using UnityEngine;

namespace ScienceAlert.Core.Gui
{
// ReSharper disable once ClassNeverInstantiated.Global
    public class CommandCreateAppLauncherView : Command
    {
        private readonly GameObject _gameContext;
        private readonly ICoroutineRunner _coroutineRunner;

        public CommandCreateAppLauncherView(
            [Name(CoreKeys.CoreContextView)] GameObject gameContext,
            ICoroutineRunner coroutineRunner)
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
