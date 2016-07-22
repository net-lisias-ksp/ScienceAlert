using UnityEngine;
using UnityEngine.UI;

namespace ScienceAlert.UI.OptionsWindow
{
    [ExecuteInEditMode]
    // ReSharper disable once UnusedMember.Global
    class ThresholdScaleController : MonoBehaviour
    {
#pragma warning disable 649
        [SerializeField] private Slider _sliderToSetMaxValueOf;
#pragma warning restore 649

        // UnityAction
        // ReSharper disable once UnusedMember.Global
        public void SetSliderMaxValueTo(float f)
        {
            if (_sliderToSetMaxValueOf == null || Mathf.Approximately(_sliderToSetMaxValueOf.maxValue, f)) return;

            if (_sliderToSetMaxValueOf.value > f)
                _sliderToSetMaxValueOf.value = f;
            _sliderToSetMaxValueOf.maxValue = f;
        }
    }
}
