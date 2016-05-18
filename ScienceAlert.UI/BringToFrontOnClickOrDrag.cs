using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

namespace ScienceAlert.UI
{
    // Bring this RectTransform in front of its siblings if it's clicked or dragged
    [RequireComponent(typeof(RectTransform)), DisallowMultipleComponent]
    class BringToFrontOnClickOrDrag : MonoBehaviour, IPointerDownHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        // Performance with the scroll rect active has proven to be terrible, so temporarily
        // disable it
        // ReSharper disable once FieldCanBeMadeReadOnly.Global
        // ReSharper disable once CollectionNeverUpdated.Global
        public List<RectTransform> ItemsToHideWhileDragging = new List<RectTransform>();

        [HideInInspector] private readonly List<KeyValuePair<RectTransform, bool>> _cachedItemStates =
            new List<KeyValuePair<RectTransform, bool>>();  

        
        public void OnPointerDown(PointerEventData eventData)
        {
            BringToFront();
            RestoreItems();
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            BringToFront();
            HideItems();
        }

        public void BringToFront()
        {
            if (transform.parent != null)
                transform.SetAsLastSibling();
        }

        public void OnDrag(PointerEventData eventData)
        {
            
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            RestoreItems();
        }


        private void HideItems()
        {
            if (_cachedItemStates.Any()) RestoreItems();

            foreach (var item in ItemsToHideWhileDragging)
            {
                if (item == null) continue;
                _cachedItemStates.Add(new KeyValuePair<RectTransform, bool>(item, item.gameObject.activeSelf));
                item.gameObject.SetActive(false);
            }
        }


        private void RestoreItems()
        {
            foreach (var pair in _cachedItemStates)
                if (pair.Key != null)
                    pair.Key.gameObject.SetActive(pair.Value);
            _cachedItemStates.Clear();
        }
    }
}
