using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using strange.extensions.context.api;
using strange.extensions.signal.impl;
using UnityEngine;
using UnityEngine.UI;
// ReSharper disable UnusedMember.Global

#pragma warning disable 649

namespace ScienceAlert.UI.ExperimentWindow
{
    [Serializable, DisallowMultipleComponent]
    public class ExperimentWindowView : ManualRegistrationView
    {
        [Inject(ContextKeys.CONTEXT)] public IContext Context { get; set; }

        [NonSerialized, HideInInspector] public readonly Signal CloseButtonClicked = new Signal();

        [SerializeField] private ExperimentListItemView _listItemPrefab;
        [SerializeField] private RectTransform _list;

        private readonly Dictionary<IExperimentIdentifier, ExperimentListItemView> _listEntries =
            new Dictionary<IExperimentIdentifier, ExperimentListItemView>();


        protected override void Awake()
        {
            _listItemPrefab.parentViewForcesInjection = false;
            _listItemPrefab.gameObject.SetActive(false);

            base.Awake();
        }


        public void UpdateExperimentEntryAlert([NotNull] IExperimentIdentifier identifier, bool alertLit)
        {
            GetListItem(identifier).AlertLit = alertLit;
        }


        // ReSharper disable once UnusedMember.Global
        public void UpdateExperimentEntry([NotNull] IExperimentIdentifier identifier, EntryDisplayStatus entryDisplayStatus)
        {
            UpdateExperimentListItem(GetListItem(identifier), entryDisplayStatus);
        }


        private ExperimentListItemView GetListItem([NotNull] IExperimentIdentifier identifier)
        {
            if (identifier == null) throw new ArgumentNullException("identifier");

            ExperimentListItemView result;

            if (_listEntries.TryGetValue(identifier, out result))
                return result;

            AddNewListItem(identifier);

            return GetListItem(identifier);
        }


        private void AddNewListItem(IExperimentIdentifier identifier)
        {
            if (_listEntries.ContainsKey(identifier))
                throw new ArgumentException("List already contains an item with identifier '" + identifier + "'",
                    "identifier");

            var instance = ExperimentListItemView.Factory.Create(_listItemPrefab, Context, identifier);

            _listEntries.Add(identifier, instance);

            instance.transform.SetParent(_list, false);
            instance.transform.SetAsLastSibling();
            instance.gameObject.SetActive(true);

            // todo: sorting

            LayoutRebuilder.MarkLayoutForRebuild(_list);
        }



        // From UnityAction
        public void OnCloseButtonClicked()
        {
            CloseButtonClicked.Dispatch();
        }


        private static void UpdateExperimentListItem([NotNull] ExperimentListItemView item, EntryDisplayStatus displayStatus)
        {
            if (item == null) throw new ArgumentNullException("item");

            item.Enabled = displayStatus.ButtonEnabled;
            item.Text = displayStatus.ExperimentTitle;

            item.RecoveryValue = displayStatus.RecoveryValue;
            item.RecoveryValueLit = displayStatus.RecoveryLit;

            item.TransmissionValue = displayStatus.TransmissionValue;
            item.TransmissionValueLit = displayStatus.TransmissionLit;

            item.LabValue = displayStatus.LabValue;
            item.LabValueLit = displayStatus.LabLit;

            item.gameObject.SetActive(displayStatus.ButtonDisplayed);
        }
    }
}
