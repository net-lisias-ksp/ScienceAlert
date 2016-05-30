using System;
using strange.extensions.signal.impl;
using UnityEngine;

namespace ScienceAlert.UI.OptionsWindow
{
    public class OptionsWindowView : ManualRegistrationView
    {
        [HideInInspector, NonSerialized] public readonly Signal CloseButtonClicked = new Signal();

        // UnityAction
        public void OnCloseButtonClicked()
        {
            CloseButtonClicked.Dispatch();
        }
    }
}
