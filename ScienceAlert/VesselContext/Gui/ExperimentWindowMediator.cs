using System;
using System.Collections.ObjectModel;
using System.Linq;
using ReeperCommon.Containers;
using ReeperCommon.Logging;
using strange.extensions.mediation.impl;
using ScienceAlert.UI.ExperimentWindow;
using UnityEngine;

// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable CollectionNeverUpdated.Global

namespace ScienceAlert.VesselContext.Gui
{
    class ExperimentWindowMediator : Mediator
    {
        private const float MinimumThresholdForIndicators = 0.1f; // sensor value must be at least this or we don't light the relevant indicator

        [Inject] public ExperimentWindowView View { get; set; }
        [Inject] public ReadOnlyCollection<ScienceExperiment> Experiments { get; set; }

        [Inject] public SignalContextIsBeingDestroyed ContextDestroyed { get; set; }

        [Inject] public SignalDeployExperiment DeployExperiment { get; set; }
        [Inject] public SignalExperimentSensorStatusChanged SensorStatusChanged { get; set; }

        public override void OnRegister()
        {
            base.OnRegister();
            Log.Warning("ExperimentWindowMediator.OnRegister");


            // todo: catch any exceptions

            //for (int i = 0; i < Experiments.Count; ++i)
            //{
            //    View.UpdateExperimentEntry(Experiments[i].id,
            //        new ExperimentEntryInfo(Experiments[i].experimentTitle, Random.Range(0f, 100f), true,
            //            Random.Range(0f, 100f), false, Random.Range(0f, 100f), true, true,
            //            i % 2 == 0), false);

            //}

            // view signals
            View.DeployButtonClicked.AddListener(OnDeployButtonClicked);

            // other signals
            SensorStatusChanged.AddListener(OnSensorStatusChanged);
            ContextDestroyed.AddOnce(OnContextDestroyed);
        }


        public override void OnRemove()
        {
            Log.Warning("ExperimentWindowMediator.OnUnregister");

            // view signals
            View.DeployButtonClicked.RemoveListener(OnDeployButtonClicked);

            // other signals
            SensorStatusChanged.RemoveListener(OnSensorStatusChanged);

            base.OnRemove();
        }




        private void OnSensorStatusChanged(SensorStatusChange status)
        {
            var newState = status.NewState;

            Log.Debug("Received status change: " + newState);

            var exp = newState.Experiment;

            var hasExperimentalValue = Mathf.Max(newState.CollectionValue, newState.TransmissionValue,
                newState.LabValue) > MinimumThresholdForIndicators;

            var buttonEnabled = hasExperimentalValue && newState.Onboard &&
                                newState.ConditionsMet && newState.Available;

            var info = new ExperimentEntryInfo(
                    /* button title */              exp.experimentTitle,
                    /* collection value */          newState.CollectionValue,
                    /* light collectioni icon? */   newState.CollectionValue > MinimumThresholdForIndicators,
                    /* transmission value */        newState.TransmissionValue,
                    /* light transmission icon? */  newState.TransmissionValue > MinimumThresholdForIndicators,
                    /* lab value */                 newState.LabValue,
                    /* light lab icon? */           newState.LabValue > MinimumThresholdForIndicators,
                    /* show in list? */             true, 
                    /* enabled? */                  buttonEnabled
                    );

            View.UpdateExperimentEntry(exp.id, info, true);
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


        private void OnContextDestroyed()
        {
            Log.Verbose(GetType().Name + " destroying view");
            View.gameObject.Do(Destroy);
        }
    }
}
