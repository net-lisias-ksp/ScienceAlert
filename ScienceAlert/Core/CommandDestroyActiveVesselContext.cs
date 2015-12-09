using System;
using ReeperCommon.Containers;
using ScienceAlert.VesselContext;
using strange.extensions.command.impl;
using strange.extensions.context.api;
using strange.extensions.injector;
using UnityEngine;

namespace ScienceAlert.Core
{
// ReSharper disable once ClassNeverInstantiated.Global
    public class CommandDestroyActiveVesselContext : Command
    {
        private readonly GameObject _coreContext;
        private readonly IVessel _destroyedVessel;

        public CommandDestroyActiveVesselContext(
            [Name(ContextKeys.CONTEXT_VIEW)] GameObject coreContext,
            IVessel destroyedVessel)
        {
            if (coreContext == null) throw new ArgumentNullException("coreContext");
            if (destroyedVessel == null) throw new ArgumentNullException("destroyedVessel");

            _coreContext = coreContext;
            _destroyedVessel = destroyedVessel;
        }


        public override void Execute()
        {
            _coreContext.GetComponentInChildren<BootstrapActiveVesselContext>()
                .With(bvc => bvc.context as ActiveVesselContext)
                .If(vc => vc.injectionBinder.GetInstance<IVessel>().Equals(_destroyedVessel))
                .Do(vc =>
                {
                    (vc.contextView as GameObject).Do(UnityEngine.Object.Destroy);
                    Log.Verbose("Destroyed vessel context");
                })
                .IfNull(Fail);
        }
    }
}
