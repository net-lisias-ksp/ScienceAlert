using System.Collections;
using strange.extensions.injector;
using strange.extensions.mediation.impl;
using UnityEngine;

namespace ScienceAlert.Gui
{
// ReSharper disable once ClassNeverInstantiated.Global
    public class AlertPanelMediator : Mediator
    {
        [Inject] public AlertPanelView View { get; set; }
        [Inject] public SignalAlertPanelViewVisibilityChanged AlertPanelVisibilitySignal { get; set; }
        [Inject] public SignalAppButtonToggled ToggledSignal { get; set; }
        [Inject] public SignalAppButtonCreated AppButtonCreatedSignal { get; set; }
        [Inject] public IRoutineRunner CoroutineRunner { get; set; }

        private Coroutine _setWindowLock; // todo: cancel this if loading settings

        public override void OnRegister()
        {
            base.OnRegister();
            
            View.LockToggle.AddListener(OnLockToggle);
            View.Close.AddListener(OnClose);

            ToggledSignal.AddListener(OnAppButtonToggled);

            AppButtonCreatedSignal.AddOnce(() => ChangeVisibility(View.Visible));

            // note to self: the window doesn't create itself until StrangeView.Start runs (and we're
            // running more or less inside awake) so we can't set lock status yet. 
            _setWindowLock = CoroutineRunner.StartCoroutine(SetInitialLockStatus());
        }


        public override void OnRemove()
        {
            base.OnRemove();
            View.LockToggle.RemoveListener(OnLockToggle);
            View.Close.RemoveListener(OnClose);

            ToggledSignal.RemoveListener(OnAppButtonToggled);
        }


        private IEnumerator SetInitialLockStatus()
        {
            yield return new WaitForEndOfFrame();
            _setWindowLock = null;

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
