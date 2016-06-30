using System;
using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
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
        
        [SerializeField] private ExperimentListEntry _listItemPrefab;
        [SerializeField] private RectTransform _list;

        [HideInInspector] private readonly Dictionary<IExperimentIdentifier, ExperimentListEntry> _listEntries =
            new Dictionary<IExperimentIdentifier, ExperimentListEntry>();


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
        public void UpdateExperimentEntry([NotNull] IExperimentIdentifier identifier, ExperimentEntryInfo entryInfo, bool resort)
        {
            UpdateExperimentListItem(GetListItem(identifier), entryInfo);
        }


        private ExperimentListEntry GetListItem([NotNull] IExperimentIdentifier identifier)
        {
            if (identifier == null) throw new ArgumentNullException("identifier");

            ExperimentListEntry result;

            if (_listEntries.TryGetValue(identifier, out result))
                return result;

            AddNewListItem(identifier);

            return GetListItem(identifier);
        }


        private void AddNewListItem(IExperimentIdentifier identifier)
        {
            if (_listEntries.ContainsKey(identifier))
                throw new ArgumentException("List already contains an entry with identifier '" + identifier + "'",
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


        private static void UpdateExperimentListItem([NotNull] ExperimentListEntry entry, ExperimentEntryInfo info)
        {
            if (entry == null) throw new ArgumentNullException("entry");

            entry.Enabled = info.ButtonEnabled;
            entry.Text = info.ExperimentTitle;

            entry.RecoveryValue = info.RecoveryValue;
            entry.RecoveryValueLit = info.RecoveryLit;

            entry.TransmissionValue = info.TransmissionValue;
            entry.TransmissionValueLit = info.TransmissionLit;

            entry.LabValue = info.LabValue;
            entry.LabValueLit = info.LabLit;

            entry.gameObject.SetActive(info.ButtonDisplayed);
        }
    }
}
