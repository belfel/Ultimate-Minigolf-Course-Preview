using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UITimer : MonoBehaviour
{
    public FloatVariable timeLeft;
    public FloatVariable phaseDuration;

    [SerializeField] private TMP_Text text;
    [SerializeField] private float displayThreshold = 0f;
    [SerializeField] private float redThreshold = 0f;

    private bool hideText;

    public void UpdateText()
    {
        if (hideText || timeLeft.value > displayThreshold)
        {
            text.text = "";
            return;
        }

        if (timeLeft.value < redThreshold)
            text.color = Color.red;
        else text.color = Color.white;

        text.text = timeLeft.value.ToString("N0");
    }

    public void SetDisplayThreshold(float threshold)
    {
        displayThreshold = threshold; 
    }

    public void SetRedThreshold(float threshold)
    {
        redThreshold = threshold;
    }

    public void ShowTimer()
    {
        hideText = false;
    }

    public void HideTimer()
    {
        hideText = true;
    }
}
