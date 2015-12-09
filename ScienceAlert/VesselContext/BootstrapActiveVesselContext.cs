using System;
using ReeperCommon.Containers;
using strange.extensions.context.impl;

namespace ScienceAlert.VesselContext
{
    public class BootstrapActiveVesselContext : ContextView
    {
// ReSharper disable once UnusedMember.Local
        public void Start()
        {
            print("BootstrapActiveVesselContext.Start");

            try
            {
                context = new ActiveVesselContext(this);
            }
            catch (Exception e)
            {
                // todo: popup dialog?
                print("Exception while boostrapping vessel context: " + e);
            }
        }


        protected override void OnDestroy()
        {
            (context as ActiveVesselContext).Do(c => c.SignalDestruction());
            base.OnDestroy();
        }
    }
}
