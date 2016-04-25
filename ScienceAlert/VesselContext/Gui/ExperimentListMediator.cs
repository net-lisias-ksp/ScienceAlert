using System;
using ReeperCommon.Containers;
using ReeperCommon.Logging;
using ReeperKSP.Extensions;
using ReeperKSP.Serialization;
using strange.extensions.mediation.impl;

namespace ScienceAlert.VesselContext.Gui
{
// ReSharper disable once ClassNeverInstantiated.Global
    public class ExperimentListMediator : Mediator
    {
        [Inject]
        public ExperimentListView View
        {
            get { return _view; }
            set { _view = value; }
        }

        [Inject] public ICoroutineRunner CoroutineRunner { get; set; }
        [Inject] public IConfigNodeSerializer Serializer { get; set; }

        [Inject(VesselContextKeys.ExperimentViewConfig)] public ConfigNode Config { get; set; }

        [Inject] public SignalLoadGuiSettings LoadSignal { get; set; }
        [Inject] public SignalSaveGuiSettings SaveSignal { get; set; }

        [Inject] public SignalExperimentSensorStatusChanged SensorChangedSignal { get; set; }
        [Inject] public SignalDeployExperiment DeployExperimentSignal { get; set; }

        private ExperimentListView _view;


        public override void OnRegister()
        {
            Log.Debug("ExperimentListMediator.OnRegister");

            base.OnRegister();

            View.LockToggle.AddListener(OnLockToggle);
            View.Close.AddListener(OnClose);
            View.DeployExperiment.AddListener(OnDeployExperiment);

            LoadSignal.AddListener(OnLoad);
            SaveSignal.AddListener(OnSave);

            SensorChangedSignal.AddListener(OnSensorStateChanged);
        }


        public override void OnRemove()
        {
            base.OnRemove();

            LoadSignal.RemoveListener(OnLoad);
            SaveSignal.RemoveListener(OnSave);

            View.LockToggle.RemoveListener(OnLockToggle);
            View.Close.RemoveListener(OnClose);
            View.DeployExperiment.RemoveListener(OnDeployExperiment);

            SensorChangedSignal.RemoveListener(OnSensorStateChanged);
        }


// ReSharper disable once UnusedMember.Local
        private void OnSave()
        {
            try
            {
                Serializer.CreateConfigNodeFromObject(_view)
                    .Do(n =>
                    {
                        Config.ClearData();
                        n.CopyTo(Config);
                    });

                Log.Verbose("Successfully serialized ExperimentListView");
                Log.Debug(() => "Serialized " + typeof(ExperimentListView).Name + " to " + Config.ToSafeString());

            }
            catch (Exception e)
            {
                Log.Error("Exception while serializing experiment view: " + e);
            }
        }


// ReSharper disable once UnusedMember.Local
        private void OnLoad()
        {
            try
            {
                Log.Debug(() => "Deserializing " + typeof (ExperimentListView).Name + " from " + Config.ToSafeString());

                if (!Config.HasData)
                {
                    Log.Warning("Cannot deserialize " + typeof (ExperimentListView).Name +
                                " from ConfigNode; no data found");
                    return;
                }

                Serializer.LoadObjectFromConfigNode(ref _view, Config);
                Log.Verbose("Successfully deserialized ExperimentListView");
            }
            catch (Exception e)
            {
                Log.Error("Exception while deserializing experiment view: " + e);
            }
        }


        private void OnLockToggle()
        {
            View.Lock(!View.Draggable);
        }


        private void OnClose()
        {
            ChangeVisibility(false);
        }


        private void OnAppButtonToggled(bool tf)
        {
            ChangeVisibility(tf);
        }


        private void ChangeVisibility(bool tf)
        {
            View.Visible = tf;
        }


        private void OnDeployExperiment(ScienceExperiment experiment)
        {
            DeployExperimentSignal.Dispatch(experiment);
        }

        private void OnSensorStateChanged(ExperimentSensorState experimentSensorState)
        {
            View.SetExperimentStatus(experimentSensorState);
        }

    }
}
