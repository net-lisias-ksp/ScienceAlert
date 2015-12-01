using System;
using System.Collections;
using ReeperCommon.Containers;
using ReeperCommon.Logging;
using ReeperCommon.Serialization;
using ReeperCommon.Serialization.Exceptions;
using ScienceAlert.Core;
using strange.extensions.injector;
using strange.extensions.mediation.impl;
using UnityEngine;

namespace ScienceAlert.Gui
{
// ReSharper disable once ClassNeverInstantiated.Global
    public class AlertPanelMediator : Mediator
    {
        [Inject]
        public AlertPanelView View
        {
            get { return _view; }
            set { _view = value; }
        }

        [Inject] public SignalAlertPanelViewVisibilityChanged AlertPanelVisibilitySignal { get; set; }
        [Inject] public SignalAppButtonToggled ToggledSignal { get; set; }
        [Inject] public SignalAppButtonCreated AppButtonCreatedSignal { get; set; }
        [Inject] public IRoutineRunner CoroutineRunner { get; set; }
        [Inject] public IConfigNodeSerializer Serializer { get; set; }
        [Inject] public IGuiConfiguration GuiConfiguration { get; set; }

        [Inject] public SignalSaveGuiSettings SaveSignal { get; set; }
        [Inject] public SignalLoadGuiSettings LoadSignal { get; set; }

        [Inject] public ILog Log { get; set; }

        private const string SaveNodeName = "AlertPanel";

        private AlertPanelView _view;

        public override void OnRegister()
        {
            base.OnRegister();

            SaveSignal.AddListener(OnSave);
            LoadSignal.AddListener(OnLoad);

            View.LockToggle.AddListener(OnLockToggle);
            View.Close.AddListener(OnClose);

            ToggledSignal.AddListener(OnAppButtonToggled);

            AppButtonCreatedSignal.AddOnce(() => ChangeVisibility(View.Visible));

            // note to self: the window doesn't create itself until StrangeView.Start runs (and we're
            // running more or less inside awake) so we can't set lock status yet. 
            CoroutineRunner.StartCoroutine(SetInitialLockStatus());
        }


        public override void OnRemove()
        {
            base.OnRemove();
            SaveSignal.RemoveListener(OnSave);
            LoadSignal.RemoveListener(OnLoad);

            View.LockToggle.RemoveListener(OnLockToggle);
            View.Close.RemoveListener(OnClose);

            ToggledSignal.RemoveListener(OnAppButtonToggled);
        }


        private void OnSave(ConfigNode configNode)
        {
            if (configNode == null) throw new ArgumentNullException("configNode");

            configNode.AddNode(Serializer.CreateConfigNodeFromObject(_view).Do(n => n.name = SaveNodeName));
        }

        private void OnLoad(ConfigNode configNode)
        {
            if (configNode == null) throw new ArgumentNullException("configNode");

            configNode.GetNode(SaveNodeName)
                .IfNull(() => Log.Warning("AlertPanel: no settings found for this window; defaults will be used"))
                .Do(n => Serializer.LoadObjectFromConfigNode(ref _view, n))
                .Do(n => Log.Debug("Loaded AlertPanel configuration"));
        }


        private IEnumerator SetInitialLockStatus()
        {
            yield return new WaitForEndOfFrame();

            View.Lock(View.Draggable);
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
            AlertPanelVisibilitySignal.Dispatch(View.Visible);
        }
    }
}
