using System.Text;
using ReeperCommon.Containers;
using ReeperCommon.Logging;
using UnityEngine;
using UnityEngine.UI;
// ReSharper disable FieldCanBeMadeReadOnly.Local
// ReSharper disable ConvertToConstant.Local

namespace ScienceAlert.UI.OptionsWindow
{
    [RequireComponent(typeof(Slider)), ExecuteInEditMode]
    class MatchTextWithNumericValue : MonoBehaviour
    {
        [SerializeField] private string _prepend = string.Empty;
        [SerializeField] private string _append = string.Empty;
        [SerializeField] private string _format = "{0:F2}";
#pragma warning disable 649
        [SerializeField] private Text _textControl;
#pragma warning restore 649

        private Slider _slider;

        private float _currentTextValue = float.MinValue;
        private readonly StringBuilder _builder = new StringBuilder();

        private void CacheSlider()
        {
            if (_slider != null) return;

            _slider = GetComponent<Slider>();

            if (_slider == null) Log.Error("Missing a slider component");

            _slider = GetComponent<Slider>().Do(s => s.onValueChanged.AddListener(OnSliderValueChanged));

            UpdateTextValue(_slider.value);
        }


        private void UpdateTextValue(float newValue)
        {
            if (_textControl == null || Mathf.Approximately(newValue, _currentTextValue)) return;

            _currentTextValue = newValue;

            _builder.Length = 0;
            _builder.Append(_prepend);
            _builder.AppendFormat(_format, newValue);
            _builder.Append(_append);

            _textControl.text = _builder.ToString();
        }


        private void OnEnable()
        {
            CacheSlider();
        }

        private void OnDisable()
        {
            _slider.Do(s => s.onValueChanged.RemoveListener(OnSliderValueChanged));
            _slider = null;
        }

        private void OnValidate()
        {
            CacheSlider();
        }

        private void Start()
        {
            CacheSlider();
        }

        private void OnSliderValueChanged(float f)
        {
            UpdateTextValue(f);
        }
    }
}
