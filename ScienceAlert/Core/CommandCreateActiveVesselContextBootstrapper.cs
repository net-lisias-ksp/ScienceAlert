using System;
using ReeperCommon.Logging;
using strange.extensions.command.impl;
using strange.extensions.context.api;
using ScienceAlert.VesselContext;
using UnityEngine;

namespace ScienceAlert.Core
{
// ReSharper disable once UnusedMember.Global
// ReSharper disable once ClassNeverInstantiated.Global
    class CommandCreateActiveVesselContextBootstrapper : Command
    {
        private const string VesselContextGameObjectName = "VesselContextView";

        private readonly GameObject _primaryContext;

        public CommandCreateActiveVesselContextBootstrapper(
            [Name(ContextKeys.CONTEXT_VIEW)] GameObject primaryContext)
        {
            if (primaryContext == null) throw new ArgumentNullException("primaryContext");
            _primaryContext = primaryContext;
        }


        public override void Execute()
        {
            if (FlightGlobals.ActiveVessel == null)
            {
                Log.Error("ActiveVessel is null; aborting vessel context bootstrap");
                Fail();
                return;
            }

            Log.Verbose("Creating vessel context for " + FlightGlobals.ActiveVessel.vesselName);

            var vesselContextGo = new GameObject(VesselContextGameObjectName + "." + FlightGlobals.ActiveVessel.vesselName, typeof (BootstrapActiveVesselContext));
            vesselContextGo.transform.parent = _primaryContext.transform;

            if (injectionBinder.GetBinding<GameObject>(CoreKeys.VesselContextView) != null)
                injectionBinder.Unbind<GameObject>(CoreKeys.VesselContextView);

            injectionBinder.Bind<GameObject>().To(vesselContextGo).ToName(CoreKeys.VesselContextView);

            Log.Verbose("Created vessel context bootstrapper");
        }
    }
}
