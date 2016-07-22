using System;
using ReeperCommon.Containers;
using ReeperCommon.Logging;
using strange.extensions.signal.impl;
using UnityEngine;

namespace ScienceAlert.UI.OptionsWindow
{
    public class OptionsWindowView : ManualRegistrationView
    {
        [SerializeField] private OptionsListItem _optionsPrefab;

        [HideInInspector, NonSerialized] public readonly Signal CloseButtonClicked = new Signal();

        [PostConstruct]
        public void TestTheThing()
        {
            Log.Warning(GetType().Name + ".Test");
        }


        private void Start()
        {
            //_optionsPrefab.gameObject.SetActive(false);
        }


        // UnityAction
        public void OnCloseButtonClicked()
        {
            CloseButtonClicked.Dispatch();
        }
    }
}
