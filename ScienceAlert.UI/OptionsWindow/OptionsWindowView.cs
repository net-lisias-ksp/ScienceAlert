using System;
using System.Collections.Generic;
using ReeperCommon.Logging;
using strange.extensions.context.api;
using strange.extensions.signal.impl;
using UnityEngine;

// ReSharper disable UnusedMember.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable MemberCanBePrivate.Global

#pragma warning disable 649
#pragma warning disable 169

namespace ScienceAlert.UI.OptionsWindow
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class OptionsWindowView : ManualRegistrationView
    {
        [SerializeField] private OptionsListItemView _optionsPrefab;
        [SerializeField] private RectTransform _list;

        [Inject(ContextKeys.CONTEXT)] public IContext Context { get; set; }

        [HideInInspector, NonSerialized] public readonly Signal CloseButtonClicked = new Signal();

        private readonly HashSet<string> _experimentTitles = new HashSet<string>();

        //[PostConstruct] // works
        //public void TestTheThing()
        //{
        //    Log.Warning(GetType().Name + ".Test");
        //}


        protected override void Start()
        {
            if (_optionsPrefab == null)
            {
                Log.Error("Options prefab not set (check AssetBundle)");
                enabled = false;
                return;
            }

            base.Start();
        }


        public void AddEntry(OptionDisplayStatus option)
        {
            if (_experimentTitles.Contains(option.ExperimentTitle))
                throw new ArgumentException("An option entry for " + option.ExperimentTitle + " has already been added");

            if (_list == null)
                throw new InvalidOperationException("List RectTransform not set");

            Log.Verbose("Adding entry: " + option.ExperimentTitle);

            _experimentTitles.Add(option.ExperimentTitle);

            var newEntry = OptionsListItemView.Factory.Create(_optionsPrefab, Context, option);

            newEntry.transform.SetParent(_list, false);
        }


        // UnityAction
        public void OnCloseButtonClicked()
        {
            CloseButtonClicked.Dispatch();
        }
    }
}
