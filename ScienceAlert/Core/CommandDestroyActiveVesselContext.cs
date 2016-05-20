using System;
using JetBrains.Annotations;
using ReeperCommon.Logging;
using strange.extensions.command.impl;
using UnityEngine;
using Object = UnityEngine.Object;

namespace ScienceAlert.Core
{
// ReSharper disable once ClassNeverInstantiated.Global
    class CommandDestroyActiveVesselContext : Command
    {
        private readonly GameObject _vesselContext;
        private readonly ICriticalShutdownEvent _failSignal;

        public CommandDestroyActiveVesselContext(
            [NotNull, Name(CrossContextKeys.VesselContextView)] GameObject vesselContext,
            [NotNull] ICriticalShutdownEvent failSignal)
        {
            if (vesselContext == null) throw new ArgumentNullException("vesselContext");
            if (failSignal == null) throw new ArgumentNullException("failSignal");

            _vesselContext = vesselContext;
            _failSignal = failSignal;
        }


        public override void Execute()
        {
            Log.Verbose("Destroying active vessel context");

            if (_vesselContext == null)
            {
                Log.Error("Vessel context is already null; something is wrong");
                Fail();
                _failSignal.Dispatch();
                return;
            }

            injectionBinder.Unbind<GameObject>(CrossContextKeys.VesselContextView);

            Log.Verbose("Destroying " + _vesselContext.name);

            Object.Destroy(_vesselContext);
        }
    }
}
