using System;
using System.Collections;
using System.Linq;
using JetBrains.Annotations;
using KSP.UI;
using ReeperCommon.Containers;
using ReeperCommon.Extensions;
using ReeperCommon.Logging;
using ReeperCommon.Utilities;
using ReeperKSP.AssetBundleLoading;
using strange.extensions.command.impl;
using strange.extensions.context.api;
using strange.extensions.mediation.api;
using ScienceAlert.UI.ExperimentWindow;
using ScienceAlert.UI.OptionsWindow;
using UnityEngine;

namespace ScienceAlert.VesselContext.Gui
{
// ReSharper disable once ClassNeverInstantiated.Global
    class CommandCreateVesselGui : Command
    {
        private readonly GameObject _contextView;
        private readonly IContext _context;
        private readonly CoroutineHoster _coroutineRunner;
        private readonly SignalCriticalShutdown _shutdownSignal;


        
#pragma warning disable 649 // is not assigned to; that's fine because it will be assigned via AssetBundleAssetLoader

        [AssetBundleAsset("assets/sciencealert/ui/sciencealertoptionswindowprefab.prefab", "sciencealert.ksp")] 
        private OptionsWindowView _optionsWindow;

        [AssetBundleAsset("assets/sciencealert/ui/sciencealertexperimentwindowprefab.prefab", "sciencealert.ksp")]
        private ExperimentWindowView _experimentWindow;

#pragma warning restore 649


        public CommandCreateVesselGui(
            [NotNull, Name(ContextKeys.CONTEXT_VIEW)] GameObject contextView, 
            [NotNull, Name(ContextKeys.CONTEXT)] IContext context,
            [NotNull] CoroutineHoster coroutineRunner, 
            [NotNull] SignalCriticalShutdown shutdownSignal)
        {
            if (contextView == null) throw new ArgumentNullException("contextView");
            if (context == null) throw new ArgumentNullException("context");
            if (coroutineRunner == null) throw new ArgumentNullException("coroutineRunner");
            if (shutdownSignal == null) throw new ArgumentNullException("shutdownSignal");

            _contextView = contextView;
            _context = context;
            _coroutineRunner = coroutineRunner;
            _shutdownSignal = shutdownSignal;
        }


        public override void Execute()
        {
            Log.Verbose("Creating VesselContext Gui");

            Retain();
            _coroutineRunner.StartCoroutine(Begin());
        }


        private IEnumerator Begin()
        {
            var loadAssetRoutine = CoroutineHoster.Instance.StartCoroutineValueless(LoadViewAssets());
            yield return loadAssetRoutine.YieldUntilComplete;

            if (loadAssetRoutine.Error.Any())
            {
                Log.Error("Failed to load view assets: " + loadAssetRoutine.Error.Value);
                Cancel();
                Release();
                yield break;
            }

            var createViewsRoutine = CoroutineHoster.Instance.StartCoroutineValueless(CreateViews());
            yield return createViewsRoutine.YieldUntilComplete;

            if (createViewsRoutine.Error.Any())
            {
                Log.Error("Failed to create views: " + createViewsRoutine.Error.Value);

                _shutdownSignal.Dispatch();
                Cancel();
                Release();
                yield break;
            }

            Release();
        }


        private IEnumerator CreateViews()
        {
            var experimentWindow = UnityEngine.Object.Instantiate(_experimentWindow);
            var optionsWindow = UnityEngine.Object.Instantiate(_optionsWindow);

            if (experimentWindow == null || optionsWindow == null)
            {
                experimentWindow.Do(UnityEngine.Object.Destroy);
                optionsWindow.Do(UnityEngine.Object.Destroy);

                throw new FailedToLoadAssetException("One or more view prefabs failed to load.");
            }

            var dialogCanvas = UIMasterController.Instance.transform.Find("DialogCanvas")
                .IfNull(() => Log.Warning("Failed to find expected dialog canvas on UIMasterController"))
                .With(t => t as RectTransform);

            _context.AddView(experimentWindow);
            _context.AddView(optionsWindow);

            foreach (var viewTransform in new [] { experimentWindow.transform, optionsWindow.transform }.Select(t => t as RectTransform))
            {
                Log.Verbose("Adding view to active vessel context: " + viewTransform.GetType().Name);

                viewTransform.Do(view => view.SetParent(dialogCanvas, false)).Do(view => view.SetAsLastSibling());
            }

            optionsWindow.gameObject.SetActive(false);

            //experimentWindow.transform.root.gameObject.PrintComponents(new DebugLog("ExperimentWindowDebug"));

            yield return null; // wait for views to start before proceeding
        }


        private IEnumerator LoadViewAssets()
        {
            Log.Verbose("Loading GUI assets from AssetBundle...");

            var loaderRoutine = AssetBundleAssetLoader.InjectAssetsAsync(this);
            yield return loaderRoutine.YieldUntilComplete;

            if (loaderRoutine.Error.Any())
                // ReSharper disable once ThrowingSystemException
                throw loaderRoutine.Error.Value;

            Log.Verbose("Finished loading view assets");
        }
    }
}
