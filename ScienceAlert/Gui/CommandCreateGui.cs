using System;
using System.Collections;
using strange.extensions.command.impl;
using strange.extensions.context.api;
using strange.extensions.injector;
using UnityEngine;

namespace ScienceAlert.Gui
{
// ReSharper disable once ClassNeverInstantiated.Global
    public class CommandCreateGui : Command
    {
        private const string GuiSettingsNodeName = "ScienceAlertGuiSettings";

        private readonly GameObject _contextView;
        private readonly IRoutineRunner _coroutineRunner;


        public CommandCreateGui([Name(ContextKeys.CONTEXT_VIEW)] GameObject contextView, IRoutineRunner coroutineRunner)
        {
            if (contextView == null) throw new ArgumentNullException("contextView");
            if (coroutineRunner == null) throw new ArgumentNullException("coroutineRunner");

            _contextView = contextView;
            _coroutineRunner = coroutineRunner;
        }


        public override void Execute()
        {
            Debug.Log("CommandCreateGui.Execute()");

            MapBindings();
            Retain();

            _coroutineRunner.StartCoroutine(CreateViews());
        }


        private IEnumerator CreateViews()
        {
            _contextView.AddComponent<ApplicationLauncherView>();
            _contextView.AddComponent<AlertPanelView>();
            _contextView.AddComponent<VesselDebugView>();

            yield return 0; // wait for views to start before proceeding
            Release();
        }


        private void MapBindings()
        {
            injectionBinder.Bind<SignalAppButtonCreated>().ToSingleton().CrossContext();
            injectionBinder.Bind<SignalAppButtonToggled>().ToSingleton().CrossContext();
            injectionBinder.Bind<SignalAlertPanelViewVisibilityChanged>().ToSingleton().CrossContext();
            injectionBinder.Bind<SignalSaveGuiSettings>().ToSingleton().CrossContext();
            injectionBinder.Bind<SignalLoadGuiSettings>().ToSingleton().CrossContext();

            injectionBinder.Bind<string>().ToValue(GuiSettingsNodeName).ToName(Keys.GuiSettingsNodeName).CrossContext();
        }
    }
}
