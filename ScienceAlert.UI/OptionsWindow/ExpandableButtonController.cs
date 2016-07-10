using System;
using System.Collections.Generic;
using UnityEngine;

namespace ScienceAlert.UI.OptionsWindow
{
    [DisallowMultipleComponent, Serializable]
    // ReSharper disable once UnusedMember.Global
    class ExpandableButtonController : MonoBehaviour
    {
        // ReSharper disable once CollectionNeverUpdated.Local
        // ReSharper disable once FieldCanBeMadeReadOnly.Local
        [SerializeField] private List<Transform> _itemsToActivateOnToggle = new List<Transform>();

        // ReSharper disable once UnusedMember.Local
        private void Awake()
        {
            foreach (var item in _itemsToActivateOnToggle)
                if (item)
                    item.gameObject.SetActive(false);
        }


        // ReSharper disable once UnusedMember.Global
        public bool Expanded
        {
            set
            {
                foreach (var item in _itemsToActivateOnToggle)
                    if (item) item.gameObject.SetActive(value);
            }
        }
    }
}
