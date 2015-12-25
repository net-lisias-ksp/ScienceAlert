using System;
using ReeperCommon.Containers;
using strange.extensions.command.impl;
using UnityEngine;

namespace ScienceAlert.Core
{
// ReSharper disable once ClassNeverInstantiated.Global
    public class CommandCriticalShutdown : Command
    {
        private readonly GameObject _coreContextView;

        public CommandCriticalShutdown([Name(CoreKeys.CoreContextView)] GameObject coreContextView)
        {
            if (coreContextView == null) throw new ArgumentNullException("coreContextView");
            _coreContextView = coreContextView;
        }


        public override void Execute()
        {
            Log.Warning("ScienceAlert shutting down due to error.");
            _coreContextView.Do(UnityEngine.Object.Destroy);
        }
    }
}
