using System;
using System.Reflection;
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
    class CommandCriticalShutdown : Command
    {
        private readonly GameObject _coreContextView;

        public CommandCriticalShutdown([Name(CoreKeys.CoreContextView)] GameObject coreContextView)
        {
            if (coreContextView == null) throw new ArgumentNullException("coreContextView");
            _coreContextView = coreContextView;
        }


        public override void Execute()
        {
            Log.Warning("ScienceAlert shutting down due to unrecoverable error.");
            PopupDialog.SpawnPopupDialog(new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), "ScienceAlert shutting down",
                "Something has gone wrong! Check the log for details", "Ok",
                true, HighLogic.UISkin);

            _coreContextView.transform.root.gameObject.PrintComponents(new DebugLog("ContextView components"));

            _coreContextView.Do(Object.Destroy);
            Assembly.GetExecutingAssembly().DisablePlugin();
        }
    }
}
