using System;
using ReeperCommon.Containers;
using strange.extensions.context.impl;

namespace ScienceAlert.VesselContext
{
    public class BootstrapActiveVesselContext : ContextView
    {
// ReSharper disable once UnusedMember.Local
        private void Awake()
        {
            Log.Debug("Bootstrapping ActiveVesselContext...");

            try
            {
                context = new ActiveVesselContext(this);
            }
            catch (Exception e)
            {
                // todo: popup dialog?
                Log.Error("Exception while boostrapping vessel context: " + e);
            }
        }


        private void Start()
        {
            try
            {
                context.Do(c => c.Launch());
            }
            catch (Exception e)
            {
                Log.Error("Exception while launching ActiveVesselContext: " + e);
            }
        }


        protected override void OnDestroy()
        {
            try
            {
                (context as ActiveVesselContext).Do(c => c.SignalDestruction(false));
                base.OnDestroy();
            }
            catch (Exception e)
            {
                Log.Error("Exception while signal ActiveVesselContext destruction: " + e);
            }
            
        }
    }
}
