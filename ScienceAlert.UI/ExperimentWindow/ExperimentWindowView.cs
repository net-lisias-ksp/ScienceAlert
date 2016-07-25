using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using JetBrains.Annotations;
using ReeperCommon.Containers;
using ReeperCommon.Logging;
using strange.extensions.signal.impl;
using UnityEngine;
using UnityEngine.UI;

#pragma warning disable 649

namespace ScienceAlert.UI.ExperimentWindow
{
    [Serializable, DisallowMultipleComponent]
    public class ExperimentWindowView : ManualRegistrationView
    {
        public enum ExperimentIndicatorTooltipType
        {
            Alert,
            Recovery,
            Transmission,
            Lab,
            None
        }


        [NonSerialized, HideInInspector]
        public readonly Signal<IExperimentIdentifier> DeployButtonClicked = new Signal<IExperimentIdentifier>();

        [NonSerialized, HideInInspector] public readonly Signal CloseButtonClicked = new Signal();

        [NonSerialized, HideInInspector] public readonly Signal<IExperimentIdentifier, ExperimentIndicatorTooltipType> ChangeTooltip = 
            new Signal<IExperimentIdentifier, ExperimentIndicatorTooltipType>();
        
        [SerializeField] private ExperimentListItem _listItemPrefab;
        [SerializeField] private RectTransform _list;

        private readonly Dictionary<IExperimentIdentifier, ExperimentListItem> _listEntries =
            new Dictionary<IExperimentIdentifier, ExperimentListItem>();


        protected override void Start()
        {
            base.Start();
            _listItemPrefab.gameObject.SetActive(false);
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


        private ExperimentListItem GetListItem([NotNull] IExperimentIdentifier identifier)
        {
            if (identifier == null) throw new ArgumentNullException("identifier");

            ExperimentListItem result;

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

            var instance = Instantiate(_listItemPrefab);

            instance.transform.SetParent(_list, false);
            instance.transform.SetAsLastSibling();
            instance.gameObject.SetActive(true);

            // todo: sorting

            _listEntries.Add(identifier, instance);

            instance.Identifier = identifier;
            instance.Deploy.AddListener(OnExperimentButtonClicked);
            instance.MousedOverIndicator.AddListener(OnIndicatorMousedOver);

            LayoutRebuilder.MarkLayoutForRebuild(_list);
        }


        private void OnExperimentButtonClicked(IExperimentIdentifier identifier)
        {
            DeployButtonClicked.Dispatch(identifier);
        }


        private void OnIndicatorMousedOver(IExperimentIdentifier experimentIdentifier, ExperimentIndicatorTooltipType tooltipType)
        {
            ChangeTooltip.Dispatch(experimentIdentifier, tooltipType);
        }


        // From UnityAction
        public void OnCloseButtonClicked()
        {
            CloseButtonClicked.Dispatch();
        }


        private static void UpdateExperimentListItem([NotNull] ExperimentListItem item, EntryDisplayStatus displayStatus)
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
