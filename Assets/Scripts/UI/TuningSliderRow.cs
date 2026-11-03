using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace HelicopterSim
{
    // Один рядок панелі тюнінгу: назва, слайдер, значення
    public class TuningSliderRow : MonoBehaviour
    {
        [SerializeField] private TMP_Text label;
        [SerializeField] private Slider slider;
        [SerializeField] private TMP_Text valueText;

        private Action<float> onChanged;

        public void Setup(string title, float min, float max, float value, Action<float> changed)
        {
            label.text = title;
            slider.minValue = min;
            slider.maxValue = max;
            slider.wholeNumbers = false;
            slider.SetValueWithoutNotify(value);
            UpdateValueText(value);

            onChanged = changed;
            slider.onValueChanged.RemoveAllListeners();
            slider.onValueChanged.AddListener(OnSliderChanged);
        }

        // Змінити значення без виклику події (для кнопки Reset)
        public void SetValue(float value)
        {
            slider.SetValueWithoutNotify(value);
            UpdateValueText(value);
        }

        private void OnSliderChanged(float value)
        {
            UpdateValueText(value);
            onChanged?.Invoke(value);
        }

        private void UpdateValueText(float value)
        {
            if (Mathf.Abs(value) >= 100f) valueText.SetText("{0:0}", value);
            else valueText.SetText("{0:2}", value);
        }
    }
}
