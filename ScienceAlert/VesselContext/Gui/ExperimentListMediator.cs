using System;
using System.Linq;
using ReeperCommon.Containers;
using ReeperCommon.Extensions;
using ReeperCommon.Serialization;
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

        private ExperimentListView _view;


        public override void OnRegister()
        {
            Log.Debug("ExperimentListMediator.OnRegister");

            base.OnRegister();

            View.LockToggle.AddListener(OnLockToggle);
            View.Close.AddListener(OnClose);

            LoadSignal.AddListener(OnLoad);
            SaveSignal.AddListener(OnSave);

            // Temp!
            Log.Warning("Using Temp code to initialize experiment list");
            ResearchAndDevelopment.GetExperimentIDs().Select(ResearchAndDevelopment.GetExperiment)
                .ToList()
                .ForEach(experiment =>
                    View.SetExperimentStatus(new ExperimentSensorState(experiment,
                        UnityEngine.Random.Range(0f, 100f),
                        UnityEngine.Random.Range(0f, 100f),
                        UnityEngine.Random.Range(0f, 100f),
                        true, true)));
        }


        public override void OnRemove()
        {
            base.OnRemove();

            LoadSignal.RemoveListener(OnLoad);
            SaveSignal.RemoveListener(OnSave);

            View.LockToggle.RemoveListener(OnLockToggle);
            View.Close.RemoveListener(OnClose);
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
    }
}
