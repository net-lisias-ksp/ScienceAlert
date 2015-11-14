using System.Diagnostics;
using strange.extensions.injector;
using strange.extensions.mediation.impl;
using Debug = UnityEngine.Debug;

namespace ScienceAlert.Gui
{
// ReSharper disable once ClassNeverInstantiated.Global
    public class AlertPanelMediator : Mediator
    {
        [Inject]
        public AlertPanelView View { get; set; }

        public override void OnRegister()
        {
            base.OnRegister();
            View.LockToggle.AddListener(OnLockToggle);
        }


        private void Start()
        {
            View.Lock(View.Draggable);
        }


        public override void OnRemove()
        {
            base.OnRemove();
            View.LockToggle.RemoveListener(OnLockToggle);
        }


        private void OnLockToggle()
        {
            View.Draggable = !View.Draggable;
        }
    }
}
