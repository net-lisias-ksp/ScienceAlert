using System;
using ReeperCommon.Containers;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
// ReSharper disable FieldCanBeMadeReadOnly.Local
#pragma warning disable 649

namespace ScienceAlert.UI.ExperimentWindow
{
    [DisallowMultipleComponent, RequireComponent(typeof(RectTransform)), Serializable]
    // ReSharper disable once ClassNeverInstantiated.Global
    public class AlertIndicatorController : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField] private Image _background;
        
        [SerializeField] public UnityEvent _enterCallbacks = new UnityEvent();
        [SerializeField] public UnityEvent _exitCallbacks = new UnityEvent();

        // ReSharper disable once InconsistentNaming
        public bool isOn
        {
            get { return _background.Return(b => b.enabled, false); }
            set { 
                if (_background)
                    _background.enabled = value;
            }
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            _enterCallbacks.Do(cb => cb.Invoke());
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            _exitCallbacks.Do(cb => cb.Invoke());
        }
    }
}
