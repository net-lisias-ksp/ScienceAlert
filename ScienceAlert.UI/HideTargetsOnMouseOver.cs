using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable CollectionNeverUpdated.Global
// ReSharper disable ConvertToConstant.Global
// ReSharper disable FieldCanBeMadeReadOnly.Global

namespace ScienceAlert.UI
{
    [RequireComponent(typeof(RectTransform)), DisallowMultipleComponent]
    class HideTargetsOnMouseOver : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        public List<RectTransform> HiddenOnMouseOver = new List<RectTransform>();
        public List<RectTransform> VisibleOnMouseOver = new List<RectTransform>();

        public float MouseOverDelaySecs = 1f;

        private bool _targetsHidden = false;
        private float _mousedOverTime = 0f;
        private bool _isMouseOver;

        // keep track of the initial states of these RectTransforms, so previously inactive
        // items won't become active later
        [HideInInspector]
        private readonly List<KeyValuePair<RectTransform, bool>> _hiddenItemsOriginalStates = new List<KeyValuePair<RectTransform, bool>>();


        public void OnPointerEnter(PointerEventData data)
        {
            _isMouseOver = true;
            _mousedOverTime = 0f;
        }


        public void OnPointerExit(PointerEventData data)
        {
            _isMouseOver = false;

            // hide the items that should be hidden and show the ones that were visible before mouse over

            if (!_targetsHidden) return;

            VisibleOnMouseOver.ForEach(rt => rt.gameObject.SetActive(false));
            Restore(_hiddenItemsOriginalStates); // hide the mouse-over-only objects
            _targetsHidden = false;
        }


        // ReSharper disable once UnusedMember.Local
        private void Update()
        {
            if (!_isMouseOver || _targetsHidden) return;

            if (Input.GetMouseButton(0)) return; // don't accumulate time while dragging

            _mousedOverTime += Time.unscaledDeltaTime;

            if (!(_mousedOverTime > MouseOverDelaySecs)) return;

            _targetsHidden = true;

            foreach (var item in HiddenOnMouseOver)
            {
                _hiddenItemsOriginalStates.Add(new KeyValuePair<RectTransform, bool>(item, item.gameObject.activeSelf));
                item.gameObject.SetActive(false);
            }

            VisibleOnMouseOver.ForEach(rt => rt.gameObject.SetActive(true));
        }


        private static void Restore(IEnumerable<KeyValuePair<RectTransform, bool>> targets)
        {
            foreach (var target in targets)
                if (target.Value)
                    target.Key.gameObject.SetActive(target.Value);
        }


    }
}
