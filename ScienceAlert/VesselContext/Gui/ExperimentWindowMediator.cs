using System;
using System.Collections.ObjectModel;
using System.Linq;
using ReeperCommon.Containers;
using ReeperCommon.Logging;
using strange.extensions.mediation.impl;
using ScienceAlert.UI;
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

        [Inject] public SignalSetTooltip TooltipSignal { get; set; }


        [Inject] public SignalDeployExperiment DeployExperiment { get; set; }
        [Inject] public SignalExperimentSensorStatusChanged SensorStatusChanged { get; set; }


        public override void OnRegister()
        {
            base.OnRegister();

            // view signals
            View.DeployButtonClicked.AddListener(OnDeployButtonClicked);
            View.ChangeTooltip.AddListener(OnChangeTooltip);
            View.CloseButtonClicked.AddListener(OnCloseButtonClicked);

            // other signals
            SensorStatusChanged.AddListener(OnSensorStatusChanged);
            ContextDestroyed.AddOnce(OnContextDestroyed);
        }


        public override void OnRemove()
        {
            // view signals
            View.DeployButtonClicked.RemoveListener(OnDeployButtonClicked);
            View.ChangeTooltip.RemoveListener(OnChangeTooltip);
            View.CloseButtonClicked.RemoveListener(OnCloseButtonClicked);

            // other signals
            SensorStatusChanged.RemoveListener(OnSensorStatusChanged);

            base.OnRemove();
        }




        private void OnSensorStatusChanged(SensorStatusChange status)
        {
            var newState = status.CurrentState;

            var exp = newState.Experiment;

            var hasExperimentalValue = Mathf.Max(newState.RecoveryValue, newState.TransmissionValue,
                newState.LabValue) > MinimumThresholdForIndicators;

            var buttonEnabled = hasExperimentalValue && newState.Onboard &&
                                newState.ConditionsMet && newState.Available;

            var info = new ExperimentEntryInfo(
                    /* button title */              exp.experimentTitle,
                    /* alerting for this exp? */    false,
                    /* recovery value */            newState.RecoveryValue,
                    /* light recovery icon? */      newState.RecoveryValue > MinimumThresholdForIndicators,
                    /* transmission value */        newState.TransmissionValue,
                    /* light transmission icon? */  newState.TransmissionValue > MinimumThresholdForIndicators,
                    /* lab value */                 newState.LabValue,
                    /* light lab icon? */           newState.LabValue > MinimumThresholdForIndicators,
                    /* show in list? */             true, 
                    /* enabled? */                  buttonEnabled
                    );

            View.UpdateExperimentEntry(new KspExperimentIdentifier(exp), info, true);
        }


        private void OnDeployButtonClicked(IExperimentIdentifier identifier)
        {
            Experiments.FirstOrDefault(exp => identifier.Equals(exp.id))
                .Do(exp => DeployExperiment.Dispatch(exp))
                .IfNull(() =>
                {
                    throw new ArgumentException("'" + identifier + "' is an unrecognized experiment identifier"); 
                });
        }


        private void OnChangeTooltip(IExperimentIdentifier identifier, ExperimentWindowView.ExperimentIndicatorTooltipType type)
        {
            TooltipSignal.Dispatch(identifier, type);
        }


        private void OnCloseButtonClicked()
        {
            View.gameObject.SetActive(false);
        }


        private void OnContextDestroyed()
        {
            Log.Verbose(GetType().Name + " destroying view");
            Destroy(gameObject);
        }
    }
}
