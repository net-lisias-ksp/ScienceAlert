using System;
using ReeperCommon.Containers;
using ReeperCommon.Logging;
using strange.extensions.context.impl;
using UnityEngine;

namespace ScienceAlert.Core
{
// ReSharper disable once ClassNeverInstantiated.Global
    class BootstrapCore : ContextView
    {
// ReSharper disable once UnusedMember.Local
        private void Awake()
        {
            try
            {
                context = new CoreContext(this);
                context.Start();
                context.Launch();
            }
            catch (Exception e)
            {
                Log.Error("ScienceAlert: Encountered an uncaught exception while creating core context: " + e);
            
#if !DEBUG
                AssemblyLoader.loadedAssemblies.GetByAssembly(Assembly.GetExecutingAssembly())
                    .Do(thisLoaded =>
                    {
                        for (int i = 0; i < AssemblyLoader.loadedAssemblies.Count; ++i)
                            if (ReferenceEquals(AssemblyLoader.loadedAssemblies[i], thisLoaded))
                            {
                                AssemblyLoader.loadedAssemblies.RemoveAt(i);
                                break;
                            }
                    });
#endif

                PopupDialog.SpawnPopupDialog(new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), "ScienceAlert unhandled exception",
                    "Encountered an unhandled exception!  See the log for details.\n\nScienceAlert has been disabled.", "Okay",
                    false, HighLogic.UISkin);

                Destroy(gameObject);
            }
        }


        protected override void OnDestroy()
        {
            (context as CoreContext).Do(c => c.SignalDestruction());
            base.OnDestroy();
        }
    }
}
