using System;
using System.Collections.ObjectModel;
using System.Linq;
using ReeperCommon.Containers;
using ReeperCommon.Logging;
using strange.extensions.mediation.impl;
using ScienceAlert.UI.ExperimentWindow;
using Random = UnityEngine.Random;
// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable CollectionNeverUpdated.Global

namespace ScienceAlert.VesselContext.Gui
{
    class ExperimentWindowMediator : Mediator
    {
        [Inject] public ExperimentWindowView View { get; set; }
        [Inject] public ReadOnlyCollection<ScienceExperiment> Experiments { get; set; }
        [Inject] public SignalDeployExperiment DeployExperiment { get; set; }

        public override void OnRegister()
        {
            base.OnRegister();
            Log.Warning("ExperimentWindowMediator.OnRegister");


            // todo: catch any exceptions

            for (int i = 0; i < Experiments.Count; ++i)
            {
                View.UpdateExperimentEntry(Experiments[i].id,
                    new ExperimentEntryInfo(Experiments[i].experimentTitle, Random.Range(0f, 100f), true,
                        Random.Range(0f, 100f), false, Random.Range(0f, 100f), true, true,
                        i % 2 == 0), false);

            }

            View.DeployButtonClicked.AddListener(OnDeployButtonClicked);
        }


        private void OnDeployButtonClicked(string identifier)
        {

            Log.Warning("ExperimentWindowMediator.OnDeployButtonClicked called with " + identifier);

            Experiments.FirstOrDefault(exp => exp.id == identifier)
                .Do(exp => DeployExperiment.Dispatch(exp))
                .IfNull(() =>
                {
                    throw new ArgumentException("'" + identifier + "' is an unrecognized experiment identifier"); 
                });
        }


        public override void OnRemove()
        {
            Log.Warning("ExperimentWindowMediator.OnUnregister");
            View.DeployButtonClicked.RemoveAllListeners();
            base.OnRemove();
        }
    }
}
