using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using strange.extensions.injector;
using strange.extensions.mediation.impl;

namespace ScienceAlert.Gui
{
    public class ApplicationLauncherMediator : Mediator
    {
        [Inject] public ApplicationLauncherView View { get; set; }
        [Inject] public SignalButtonCreated ButtonCreated { get; set; }


        public override void OnRegister()
        {
            base.OnRegister();
            View.ButtonCreated.AddOnce(OnButtonCreated);
            View.Toggle.AddListener(OnButtonToggle);
        }

        public override void OnRemove()
        {
            base.OnRemove();
            View.Toggle.RemoveListener(OnButtonToggle);
        }


        private void OnButtonCreated()
        {
            ButtonCreated.Dispatch();
        }


        private void OnButtonToggle(bool b)
        {
            // todo:
        }
    }
}
