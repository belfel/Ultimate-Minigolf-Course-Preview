using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SettingsSlider : MonoBehaviour
{
    public FloatVariable variable;
    [SerializeField] private Slider slider;
    [SerializeField] private TMP_Text value;
    [SerializeField] private bool wholeNumbers;
    [SerializeField] private bool divideByHundred;

    private void Start()
    {
        if (divideByHundred)
            slider.value = variable.value * 100f;
        else slider.value = variable.value;
        OnValueChange();
    }

    public void OnValueChange()
    {
        if (divideByHundred)
            variable.SetValue(slider.value / 100f);
        else variable.SetValue(slider.value);

        if (wholeNumbers)
            value.text = ((int)slider.value).ToString();
        else value.text = slider.value.ToString("0.0");
    }
}
