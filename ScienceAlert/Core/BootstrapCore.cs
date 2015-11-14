using System;
using strange.extensions.context.impl;
using UnityEngine;

namespace ScienceAlert.Core
{
// ReSharper disable once ClassNeverInstantiated.Global
    public class BootstrapCore : ContextView
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
                Debug.LogError("ScienceAlert: Encountered an uncaught exception while creating core context: " + e);
            
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

                Debug.LogError("Issuing destruction call");

                PopupDialog.SpawnPopupDialog("ScienceAlert unhandled exception",
                    "Encountered an unhandled exception!  See the log for details.\n\nScienceAlert has been disabled.", "Okay",
                    false, HighLogic.Skin);

                Destroy(gameObject);
            }
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            print("BootstrapCore.OnDestroy");
        }
    }
}
