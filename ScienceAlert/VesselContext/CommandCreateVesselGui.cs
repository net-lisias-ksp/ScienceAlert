using System;
using System.Collections;
using ScienceAlert.Core;
using ScienceAlert.Gui;
using ScienceAlert.VesselContext.Gui;
using strange.extensions.command.impl;
using strange.extensions.context.api;
using strange.extensions.injector;
using UnityEngine;

namespace ScienceAlert.VesselContext
{
// ReSharper disable once ClassNeverInstantiated.Global
    public class CommandCreateVesselGui : Command
    {
        private readonly GameObject _contextView;
        private readonly SignalLoadGuiSettings _loadGuiSettings;
        private readonly ICoroutineRunner _coroutineRunner;


        public CommandCreateVesselGui(
            [Name(ContextKeys.CONTEXT_VIEW)] GameObject contextView, 
            SignalLoadGuiSettings loadGuiSettings,
            ICoroutineRunner coroutineRunner)
        {
            if (contextView == null) throw new ArgumentNullException("contextView");
            if (loadGuiSettings == null) throw new ArgumentNullException("loadGuiSettings");
            if (coroutineRunner == null) throw new ArgumentNullException("coroutineRunner");

            _contextView = contextView;
            _loadGuiSettings = loadGuiSettings;
            _coroutineRunner = coroutineRunner;
        }


        public override void Execute()
        {
            Retain();
            _coroutineRunner.StartCoroutine(CreateViews());
        }


        private IEnumerator CreateViews()
        {
            _contextView.AddComponent<ExperimentView>();
            _contextView.AddComponent<VesselDebugView>();

            yield return 0; // wait for views to start before proceeding

            _loadGuiSettings.Dispatch();

            Release();
        }
    }
}
