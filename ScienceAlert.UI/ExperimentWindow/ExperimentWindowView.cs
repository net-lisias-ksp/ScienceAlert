using System;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.UI;

#pragma warning disable 649

namespace ScienceAlert.UI.ExperimentWindow
{
    [Serializable, DisallowMultipleComponent]
    public class ExperimentWindowView : ManualRegistrationView
    {
        [SerializeField] private ExperimentListEntry _listItemPrefab;
        [SerializeField] private RectTransform _list;

        //[NonSerialized, HideInInspector] internal readonly Signal<IExperimentEntry> DeployExperiment = new Signal<IExperimentEntry>();
 
        // ReSharper disable once UnusedMember.Global
        public void AddExperimentEntry([NotNull] IExperimentEntry entry)
        {
            if (entry == null) throw new ArgumentNullException("entry");
            if (_listItemPrefab == null) throw new InvalidOperationException("Missing list item prefab");
            if (_list == null) throw new InvalidOperationException("Missing list reference");

            var instance = Instantiate(_listItemPrefab);

            instance.Experiment = entry;
            //instance.Deploy.AddListener(OnDeployButtonClicked);

            instance.transform.parent = _list;
            instance.transform.SetAsLastSibling();


            LayoutRebuilder.MarkLayoutForRebuild(_list);
        }


        public void RemoveExperimentEntry([NotNull] IExperimentEntry entry)
        {
            if (entry == null) throw new ArgumentNullException("entry");
            if (_list == null) throw new InvalidOperationException("Missing list reference");

            throw new NotImplementedException();
        }


        //private void OnDeployButtonClicked([NotNull] IExperimentEntry entry)
        //{
        //    if (entry == null) throw new ArgumentNullException("entry");

        //    Log.Warning("Experiment button: " + entry.experimentID + " clicked");

        //    DeployExperiment.Dispatch(entry);
        //}
    }
}
