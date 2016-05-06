using System;
using ReeperCommon.Logging;
using ScienceAlert.VesselContext;
using strange.extensions.command.impl;
using strange.extensions.context.api;
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

            var vesselContextGo = new GameObject(VesselContextGameObjectName, typeof (BootstrapActiveVesselContext));
            vesselContextGo.transform.parent = _primaryContext.transform;


            Log.Verbose("Created vessel context bootstrapper");
        }
    }
}
