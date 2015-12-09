using System;
using System.Collections;
using ReeperCommon.Containers;
using ReeperCommon.Serialization;
using strange.extensions.injector;
using strange.extensions.mediation.impl;
using UnityEngine;

namespace ScienceAlert.VesselContext.Gui
{
// ReSharper disable once ClassNeverInstantiated.Global
    public class ExperimentMediator : Mediator
    {
        [Inject]
        public ExperimentView View
        {
            get { return _view; }
            set { _view = value; }
        }

        [Inject] public ICoroutineRunner CoroutineRunner { get; set; }
        [Inject] public IConfigNodeSerializer Serializer { get; set; }

        [Inject(VesselKeys.ExperimentViewConfig)] public ConfigNode Config { get; set; }

        [Inject] public SignalLoadGuiSettings LoadSignal { get; set; }
        [Inject] public SignalSaveGuiSettings SaveSignal { get; set; }

        private ExperimentView _view;


        public override void OnRegister()
        {
            base.OnRegister();

            SaveSignal.AddListener(OnSave);
            LoadSignal.AddListener(OnLoad);

            View.LockToggle.AddListener(OnLockToggle);
            View.Close.AddListener(OnClose);
        }


        public override void OnRemove()
        {
            base.OnRemove();
            SaveSignal.RemoveListener(OnSave);
            LoadSignal.RemoveListener(OnLoad);

            View.LockToggle.RemoveListener(OnLockToggle);
            View.Close.RemoveListener(OnClose);
        }


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

                Log.Verbose("Successfully serialized ExperimentView");
            }
            catch (Exception e)
            {
                Log.Error("Exception while serializing experiment view: " + e);
            }
        }


        private void OnLoad()
        {
            try
            {
                Serializer.LoadObjectFromConfigNode(ref _view, Config);
                Log.Verbose("Successfully deserialized ExperimentView");
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
            //AlertPanelVisibilitySignal.Dispatch(View.Visible);
        }
    }
}
