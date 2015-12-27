﻿using System;
using System.Collections;
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
        private readonly ICoroutineRunner _coroutineRunner;


        public CommandCreateVesselGui(
            [Name(ContextKeys.CONTEXT_VIEW)] GameObject contextView, 
            ICoroutineRunner coroutineRunner)
        {
            if (contextView == null) throw new ArgumentNullException("contextView");
            if (coroutineRunner == null) throw new ArgumentNullException("coroutineRunner");

            _contextView = contextView;
            _coroutineRunner = coroutineRunner;
        }


        public override void Execute()
        {
            Log.Verbose("Creating VesselContext Gui");

            Retain();
            _coroutineRunner.StartCoroutine(CreateViews());
        }


        private IEnumerator CreateViews()
        {
            var guiGo = new GameObject("VesselGuiView");
            guiGo.transform.parent = _contextView.transform;

            injectionBinder.Bind<GameObject>().ToValue(guiGo).ToName(VesselContextKeys.GuiContainer);

            Log.Debug("Adding ExperimentView");
            guiGo.AddComponent<ExperimentView>();

            Log.Debug("Adding VesselDebugView");
            guiGo.AddComponent<VesselDebugView>();

            yield return 0; // wait for views to start before proceeding

            Release();
        }
    }
}
