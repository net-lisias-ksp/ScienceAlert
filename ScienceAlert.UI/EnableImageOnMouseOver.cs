using ReeperCommon.Containers;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ScienceAlert.UI
{
    [RequireComponent(typeof(RectTransform))]
    class EnableImageOnMouseOver : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField] private Image _targetImage;

        // todo: check the IL and make sure the extension method isn't generated garbage on every mouseenter and mouseexit

        private void Awake()
        {
            _targetImage.enabled = false;
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            _targetImage.Do(img => img.enabled = true);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            _targetImage.Do(img => img.enabled = false);
        }
    }
}
