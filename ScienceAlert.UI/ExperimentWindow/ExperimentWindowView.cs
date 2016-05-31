using System;
using System.Collections.Generic;
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
            Collection,
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


        // ReSharper disable once UnusedMember.Global
        public void UpdateExperimentEntry([NotNull] IExperimentIdentifier identifier, ExperimentEntryInfo entryInfo, bool resort)
        {
            if (identifier == null) throw new ArgumentNullException("identifier");
            if (_listItemPrefab == null) throw new InvalidOperationException("Missing list item prefab");
            if (_list == null) throw new InvalidOperationException("Missing list reference");

            
            bool rebuildLayout = resort;

            var target = GetListItem(identifier).Or(() =>
            {
                rebuildLayout = true;
                AddNewListItem(identifier);
                return GetListItem(identifier).Value;
            });


            UpdateExperimentListItem(target, entryInfo);

            if (rebuildLayout) LayoutRebuilder.MarkLayoutForRebuild(_list);
        }


        private Maybe<ExperimentListEntry> GetListItem(IExperimentIdentifier identifier)
        {
            ExperimentListEntry result;

            return !_listEntries.TryGetValue(identifier, out result) ? Maybe<ExperimentListEntry>.None : result.ToMaybe();
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


        private static void UpdateExperimentListItem(ExperimentListEntry entry, ExperimentEntryInfo info)
        {
            entry.Enabled = info.ButtonEnabled;
            entry.Text = info.ExperimentTitle;
            
            entry.CollectionValue = info.CollectionValue;
            entry.CollectionAlertLit = info.CollectionAlertLit;

            entry.TransmissionValue = info.TransmissionValue;
            entry.TransmissionAlertLit = info.TransmissionAlertLit;

            entry.LabValue = info.LabValue;
            entry.LabAlertLit = info.LabAlertLit;

            entry.gameObject.SetActive(info.ButtonDisplayed);
        }
    }
}
