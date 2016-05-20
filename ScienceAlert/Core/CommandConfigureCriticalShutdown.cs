using System;
using System.Reflection;
using JetBrains.Annotations;
using ReeperCommon.Containers;
using ReeperCommon.Extensions;
using ReeperCommon.Logging;
using ReeperKSP.Extensions;
using strange.extensions.command.impl;
using UnityEngine;
using Object = UnityEngine.Object;

namespace ScienceAlert.Core
{
// ReSharper disable once ClassNeverInstantiated.Global
    class CommandConfigureCriticalShutdown : Command
    {
        private readonly GameObject _coreContextView;
        private readonly ICriticalShutdownEvent _criticalShutdownEvent;

        public CommandConfigureCriticalShutdown([Name(CrossContextKeys.CoreContextView)] GameObject coreContextView,
            [NotNull] ICriticalShutdownEvent criticalShutdownEvent)
        {
            if (coreContextView == null) throw new ArgumentNullException("coreContextView");
            if (criticalShutdownEvent == null) throw new ArgumentNullException("criticalShutdownEvent");
            _coreContextView = coreContextView;
            _criticalShutdownEvent = criticalShutdownEvent;
        }


        public override void Execute()
        {
            var subscription = _criticalShutdownEvent.Subscribe(() =>
            {
                Log.Warning("ScienceAlert shutting down due to unrecoverable error.");
                PopupDialog.SpawnPopupDialog(new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                    "ScienceAlert shutting down",
                    "Something has gone wrong! Check the log for details", "Ok",
                    true, HighLogic.UISkin);

                _coreContextView.transform.root.gameObject.PrintComponents(new DebugLog("ContextView components"));

                _coreContextView.Do(Object.Destroy);
                Assembly.GetExecutingAssembly().DisablePlugin();
            });

            injectionBinder.Bind<IDisposable>().To(subscription).ToName(CoreContextKeys.CoreContextShutdownEventSubscription);
        }
    }
}
