using ReeperCommon.Logging;
using strange.extensions.mediation.impl;
using ScienceAlert.UI.ExperimentWindow;
using UnityEngine;

namespace ScienceAlert.VesselContext.Gui
{
    class ExperimentWindowMediator : Mediator
    {
        [Inject] public ExperimentWindowView View { get; set; }

        public override void OnRegister()
        {
            base.OnRegister();
            Log.Warning("ExperimentWindowMediator.OnRegister");

            for (int i = 0; i < 30; ++i)
            {
                View.UpdateExperimentEntry("identifier" + i,
                    new ExperimentEntryInfo("Title" + i, UnityEngine.Random.Range(0f, 100f), true,
                        UnityEngine.Random.Range(0f, 100f), false, UnityEngine.Random.Range(0f, 100f), true, true,
                        i % 2 == 0), false);

            }
        }

        public override void OnRemove()
        {
            Log.Warning("ExperimentWindowMediator.OnUnregister");
            base.OnRemove();
        }
    }
}
