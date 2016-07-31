using System;
using ReeperCommon.Containers;
using ReeperCommon.Logging;
using strange.extensions.signal.impl;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// ReSharper disable FieldCanBeMadeReadOnly.Local
// ReSharper disable FieldCanBeMadeReadOnly.Global
#pragma warning disable 649

namespace ScienceAlert.UI.ExperimentWindow
{
    [DisallowMultipleComponent, RequireComponent(typeof(RectTransform)), Serializable]
    // ReSharper disable once ClassNeverInstantiated.Global
    public class IndicatorController : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField] private Image _background;

        // ReSharper disable once MemberCanBePrivate.Global
        [NonSerialized] internal readonly Signal<bool> MouseEvent = new Signal<bool>(); 

 
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
            MouseEvent.Dispatch(true);
        }


        public void OnPointerExit(PointerEventData eventData)
        {
            MouseEvent.Dispatch(false);
        }
    }
}
