using System;
using System.Collections.ObjectModel;
using System.Linq;
using ReeperCommon.Containers;
using ReeperCommon.Logging;
using ReeperKSP.Serialization;
using strange.extensions.mediation.impl;
using ScienceAlert.UI;
using ScienceAlert.UI.ExperimentWindow;
using ScienceAlert.VesselContext.Experiments;
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
        [Inject] public IAlertStateCache AlertCache { get; set; }
        [Inject] public ExperimentIdentifierProvider IdentifierProvider { get; set; }
        [Inject] public SignalContextIsBeingDestroyed ContextDestroyed { get; set; }
        [Inject(CrossContextKeys.ExperimentWindowConfig)] public ConfigNode Configuration { get; set; }
        [Inject] public IConfigNodeSerializer Serializer { get; set; }

        [Inject] public SignalSetTooltip TooltipSignal { get; set; }



        [Inject] public SignalExperimentSensorStatusChanged SensorStatusChanged { get; set; }
        [Inject] public SignalExperimentAlertChanged AlertStatusChanged { get; set; }

        [Inject] public SignalSaveGuiSettings SaveSignal { get; set; }
        [Inject] public SignalLoadGuiSettings LoadSignal { get; set; }


        public override void OnRegister()
        {
            base.OnRegister();

            // view signals
            View.CloseButtonClicked.AddListener(OnCloseButtonClicked);

            // other signals
            SensorStatusChanged.AddListener(OnSensorStatusChanged);
            AlertStatusChanged.AddListener(OnAlertStatusChanged);
            LoadSignal.AddListener(OnGuiLoadSignal);
            SaveSignal.AddListener(SaveViewConfiguration);

            ContextDestroyed.AddOnce(OnContextDestroyed);

            // load signal should be sent shortly
        }


        public override void OnRemove()
        {
            // view signals
            View.CloseButtonClicked.RemoveListener(OnCloseButtonClicked);

            // other signals
            SensorStatusChanged.RemoveListener(OnSensorStatusChanged);
            AlertStatusChanged.RemoveListener(OnAlertStatusChanged);
            LoadSignal.RemoveListener(OnGuiLoadSignal);
            SaveSignal.RemoveListener(SaveViewConfiguration);

            base.OnRemove();
        }


        private void SaveViewConfiguration()
        {
            try
            {
                var view = View;
                var saved = Serializer.CreateConfigNodeFromObject(view);

                Configuration.ClearData();
                ConfigNode.Merge(Configuration, saved);
                Configuration.name = SharedConfiguration.ExperimentWindowConfigNodeName; // because Merge overwrites it for some reason

                Log.Verbose("Saved experiment view configuration");
            }
            catch (Exception e)
            {
                Log.Error("Error while saving view configuration: " + e);
            }
        }



        private void OnGuiLoadSignal()
        {
            try
            {
                var view = View;
                Serializer.LoadObjectFromConfigNode(ref view, Configuration);
                Log.Verbose("Loading experiment view configuration");
            }
            catch (Exception e)
            {
                Log.Error("Error while loading view configuration: " + e);
            }
       }


        private void OnSensorStatusChanged(SensorStatusChange status)
        {
            var newState = status.CurrentState;
            var exp = newState.Experiment;
            var identifier = IdentifierProvider.Get(status.CurrentState.Experiment);

            var hasExperimentalValue = Mathf.Max(newState.RecoveryValue, newState.TransmissionValue,
                newState.LabValue) > MinimumThresholdForIndicators;

            var buttonEnabled = hasExperimentalValue && newState.Onboard &&
                                newState.ConditionsMet && newState.Available;

            var info = new EntryDisplayStatus(
                    /* button title */              exp.experimentTitle,
                    /* alerting for this exp? */    AlertCache.GetStatus(identifier) != ExperimentAlertStatus.None,
                    /* recovery value */            newState.RecoveryValue,
                    /* light recovery icon? */      newState.RecoveryValue > MinimumThresholdForIndicators,
                    /* transmission value */        newState.TransmissionValue,
                    /* light transmission icon? */  newState.TransmissionValue > MinimumThresholdForIndicators,
                    /* lab value */                 newState.LabValue,
                    /* light lab icon? */           newState.LabValue > MinimumThresholdForIndicators,
                    /* show in list? */             true, 
                    /* enabled? */                  buttonEnabled
                    );

            View.UpdateExperimentEntry(identifier, info);
        }


        private void OnAlertStatusChanged(SensorStatusChange sensorStatus, AlertStatusChange alertStatus)
        {
            View.UpdateExperimentEntryAlert(IdentifierProvider.Get(sensorStatus.CurrentState.Experiment),
                alertStatus.CurrentStatus != ExperimentAlertStatus.None);
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
