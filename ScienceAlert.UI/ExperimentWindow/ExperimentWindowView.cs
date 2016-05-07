using System;
using System.Collections.Generic;
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
        [NonSerialized, HideInInspector] public readonly Signal<string> DeployButtonClicked = new Signal<string>();
        
        [SerializeField] private ExperimentListEntry _listItemPrefab;
        [SerializeField] private RectTransform _list;

        //[NonSerialized, HideInInspector] internal readonly Signal<ExperimentEntryInfo> DeployExperiment = new Signal<ExperimentEntryInfo>();

        [HideInInspector] private readonly Dictionary<string, ExperimentListEntry> _listEntries =
            new Dictionary<string, ExperimentListEntry>();
 

        // ReSharper disable once UnusedMember.Global
        public void UpdateExperimentEntry(string identifier, ExperimentEntryInfo entryInfo, bool resort)
        {
            if (string.IsNullOrEmpty(identifier)) throw new ArgumentException("cannot be null or empty", "identifier");
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


        private Maybe<ExperimentListEntry> GetListItem(string identifier)
        {
            ExperimentListEntry result;

            return !_listEntries.TryGetValue(identifier, out result) ? Maybe<ExperimentListEntry>.None : result.ToMaybe();
        }


        private void AddNewListItem(string identifier)
        {
            if (_listEntries.ContainsKey(identifier))
                throw new ArgumentException("List already contains an entry with identifier '" + identifier + "'",
                    "identifier");

            var instance = Instantiate(_listItemPrefab);

            instance.transform.SetParent(_list, false);
            instance.transform.SetAsLastSibling();

            // todo: sorting

            _listEntries.Add(identifier, instance);

            instance.Deploy.AddListener(() => OnExperimentButtonClicked(identifier));
        }


        private void OnExperimentButtonClicked(string identifier)
        {
            Log.Warning("ExperimentWindowView.OnExperimentButtonClicked: " + identifier);
            DeployButtonClicked.Dispatch(identifier);
        }


        private void UpdateExperimentListItem(ExperimentListEntry entry, ExperimentEntryInfo info)
        {
            entry.Enabled = info.ButtonEnabled;
            entry.Text = info.ExperimentTitle;
            
            entry.CollectionValue = info.CollectionValue;
            entry.CollectionAlertLit = info.CollectionAlertLit;

            entry.TransmissionValue = info.TransmissionValue;
            entry.TransmissionAlertLit = info.TransmissionAlertLit;

            entry.LabValue = info.LabValue;
            entry.LabAlertLit = info.LabAlertLit;

        }
    }
}
